using BSMConsole.Themes;

namespace BSMConsole;

internal class BattleshipGame
{
	// Settings
	public GameType GameType        { get; set; } = GameType.Classic;
	public bool     NetworkPlay     { get; set; } = false;
	public string   PlayerName      { get; set; } = "Me";
	public bool     RandomPlacement { get; set; } = false;
	public ITheme   Theme           { get; set; } = new DefaultTheme();
	public string   Uri             { get; set; } = "";

	// Game layout constants
	private const int CLEAR_WIDTH = 56;
	private const int GAME_HEIGHT = 22;
	private const int GAME_WIDTH  = 66;
	private const int BOARD_ROW   =  2;
	private const int LEFT_GRID   =  4;
	private const int RIGHT_GRID  = 34;
	private const int INPUT_COL   = LEFT_GRID;
	private const int INPUT_ROW   = 18;
	private const int STATUS_COL  = LEFT_GRID;
	private const int STATUS_ROW  = 17;


	static readonly object consoleDisplayLock = new();
	private int _topRow = int.MinValue;
	
	private readonly List<AttackResult> _attackResults = new();
	private Dictionary<ShipType, Ship>  _myFleet       = new();

	private AuthPlayer     player   = new("Me");
	private ComputerPlayer opponent = new("Computer");

	internal void Play()
	{
		GameStatus gameStatus = GameStatus.AddingPlayers;

		player = player with { Name = PlayerName };
		List<Player> players = new()
		{
			player,
			opponent
		};
		Game game = Game.StartNewGame(players, GameType);

		_topRow = PrepareGameSpace();
		DisplayGame();

		gameStatus = GameStatus.PlacingShips;
		DisplayStatus(gameStatus);

		List<Ship> ships = Game.GameShips(GameType);
		if (RandomPlacement is false) {
			DisplayEmptyGrid(player, game.BoardSize);
			ships = PlaceShips(ships);
			if (ships is null) {
				gameStatus = GameStatus.Abandoned;
				DisplayStatus(gameStatus);
				Console.SetCursorPosition(0, _topRow + GAME_HEIGHT - 2);
				Console.WriteLine();
				Console.WriteLine();
				return;
			}
		}
		game.PlaceShips(player, ships, doItForMe: RandomPlacement);
		_myFleet = game.Fleet(player).ToDictionary(ship => ship.Type, ship => ship);

		if (game.AreFleetsReady) {
			gameStatus = GameStatus.Attacking;
			DisplayStatus(gameStatus);
			DisplayBoards(player, opponent, _myFleet.Values, game.BoardSize);

			while (game.GameOver is false) {
				if (TryGetCoordinateFromUser(_topRow + INPUT_ROW, Theme.Colour, out Coordinate coordinate)) {
					_attackResults.Add(game.Fire(player, coordinate));
					_attackResults.AddRange(game.OtherPlayersFire());
					DisplayBoards(player, opponent, _myFleet.Values, game.BoardSize);
				} else {
					gameStatus = GameStatus.Abandoned;
					break;
				}
			}
		}

		gameStatus = game.GameOver ? GameStatus.GameOver : GameStatus.Abandoned;
		DisplayStatus(gameStatus);

		DisplayFinalSummary(game.LeaderBoard());
	}

	internal async Task PlayNetworkGame()
	{
		Game game = new Game();
		GameId gameId;
		GameStatus gameStatus = GameStatus.AddingPlayers;

		await using HubConnection hubConnection = new HubConnectionBuilder()
			.WithUrl($"{Uri}/bsm")
			.WithAutomaticReconnect()
			.Build();

		await hubConnection.StartAsync();

		// ToDo: Can reintroduce this when I can serialize all types of Player

		//_hubConnection.On<Player>("Opponents", (player) => {
		//	Debug.WriteLine($"Opponent: {player}");
		//	opponent = player is ComputerPlayer p ? p : throw new ApplicationException("No opponents found.");
		//});

		hubConnection.On<GameId>("StartGame", (gId) => gameId = gId);
		hubConnection.On<GameStatus>("GameStatusChange", (gStat) => { gameStatus = gStat; DisplayStatus(gameStatus); });
		hubConnection.On<List<AttackResult>>("AttackResults", (attackResults) => {
			foreach (AttackResult attackResult in attackResults.Where(ar => ar.TargetedPlayerId == player.Id)) {
				if (attackResult.ShipType is ShipType s) {
					_myFleet[s].Attack(attackResult.AttackCoordinate);
				}
			}
			_attackResults.AddRange(attackResults);
			DisplayShotsOnGrid(opponent);
			DisplayShotsOnGrid(player, RIGHT_GRID);
		});

		try {
			player = await hubConnection.InvokeAsync<AuthPlayer>("RegisterPlayer", PlayerName);
		}
		catch (Exception) {
			throw;
		}

		_topRow = PrepareGameSpace();
		DisplayGame();

		try {
			gameId   = await hubConnection.InvokeAsync<GameId>("StartGameVsComputer", player, "Computer", game.GameType);
			opponent = await hubConnection.InvokeAsync<ComputerPlayer>("FindComputerOpponent", player, gameId);
		}
		catch (Exception) {
			throw;
		}

		List<Ship> ships = Game.GameShips(GameType);
		if (RandomPlacement is false) {
			DisplayEmptyGrid(player, game.BoardSize);
			ships = PlaceShips(ships);
			if (ships is null) {
				gameStatus = GameStatus.Abandoned;
				DisplayStatus(gameStatus);
				Console.SetCursorPosition(0, _topRow + GAME_HEIGHT - 2);
				Console.WriteLine();
				Console.WriteLine();
				return;
			}
		}
		_myFleet = (await
				hubConnection
				.InvokeAsync<List<Ship>>("PlaceShips", player, gameId, ships, RandomPlacement))
			.ToDictionary(ship => ship.Type, ship => ship);

		// ToDo: This is a hack - work out how to wait efficiently for the GameStatus to change to Attacking
		Task.Delay(500).Wait();

		if (gameStatus is GameStatus.Attacking) {
			DisplayStatus(gameStatus);
			DisplayBoards(player, opponent, _myFleet.Values, game.BoardSize);

			while (gameStatus is GameStatus.Attacking) {
				if (TryGetCoordinateFromUser(_topRow + INPUT_ROW, Theme.Colour, out Coordinate coordinate)) {
					List<AttackResult> attackResults = await hubConnection
						.InvokeAsync<List<AttackResult>>("Fire", player, gameId, coordinate);
				} else {
					gameStatus = GameStatus.Abandoned;
					break;
				}
			}
		}

		//_ = await hubConnection.InvokeAsync<bool>("UnRegisterPlayer", player);

		DisplayStatus(gameStatus);

		List<LeaderboardEntry> leaderboard = await hubConnection.InvokeAsync<List<LeaderboardEntry>>("Leaderboard", gameId);
		DisplayFinalSummary(leaderboard);
	}

	private List<Ship> PlaceShips(IEnumerable<Ship> myFleet)
	{
		List<Ship> fleet = myFleet.ToList();
		List<Ship> newFleet = myFleet.ToList();

		for (int i = 0; i < fleet.Count; i++) {
			Ship ship = fleet[i];
			bool badPlacement = true;
			do {
				DisplayShipsOnGrid(fleet);
				DisplayStatus(GameStatus.PlacingShips, $" [green]{ship.Type.ToFriendlyString()}[/] ({ship.NoOfSegments} segments) ");
				(Orientation Orientation, Coordinate Coordinate)? result = GetShipPlacementFromUser(_topRow + INPUT_ROW, Theme.Colour);

				// User probably pressed Esc
				if (result.HasValue is false) {
					return null!;
				}

				newFleet[i] = new(ship.Type, result.Value.Coordinate, result.Value.Orientation);
				if (Board.ValidateShipPositions(newFleet)) {
					badPlacement = false;
					fleet[i] = new(ship.Type, result.Value.Coordinate, result.Value.Orientation);
				}

			} while (badPlacement);
		}
		return fleet.ToList();
	}

	private void DisplayBoards(Player player, Player opponent, IEnumerable<Ship> myFleetOfShips, int boardSize)
	{
		DisplayEmptyGrid(opponent, boardSize);
		DisplayShotsOnGrid(opponent);

		DisplayEmptyGrid(player, boardSize, offsetCol: RIGHT_GRID);
		DisplayShotsOnGrid(player, RIGHT_GRID);
		DisplayShipsOnGrid(myFleetOfShips, RIGHT_GRID);
	}

	private void DisplayGame()
	{
		AnsiConsole.Background = Console.BackgroundColor;
		AnsiConsole.Foreground = Console.ForegroundColor;
		Theme.BackgroundColour = Theme.BackgroundColour == "default" ? AnsiConsole.Background.ToMarkup() : Theme.BackgroundColour;
		Theme.ForegroundColour = Theme.ForegroundColour == "default" ? AnsiConsole.Foreground.ToMarkup() : Theme.ForegroundColour;

		Console.SetCursorPosition(0, _topRow);
		AnsiConsole.Markup($"[{Theme.Colour}]┌{new string('─', GAME_WIDTH - 2)}┐[/]");
		Console.WriteLine();
		for (int row = 0; row < GAME_HEIGHT - 4; row++) {
			AnsiConsole.Markup($"[{Theme.Colour}]│{new string(' ', GAME_WIDTH - 2)}│[/]");
			Console.WriteLine();
		}
		AnsiConsole.Markup($"[{Theme.Colour}]└{new string('─', GAME_WIDTH - 2)}┘[/]");
		Console.WriteLine();

		Console.SetCursorPosition(3, _topRow);
		AnsiConsole.Markup($"[{ Theme.Colour}] T H E   G A M E   O F   B A T T L E S H I P [/]");

		//Console.SetCursorPosition(3, _topRow + 1);
		//Console.Write($"Colour: {Theme.Colour} = {AnsiConsole.Foreground} on {AnsiConsole.Background}");
	}

	private void DisplayEmptyGrid(Player player, int boardSize, int offsetCol = 4)
	{
		int offsetRow = _topRow + BOARD_ROW;

		Console.SetCursorPosition(offsetCol, offsetRow);
		AnsiConsole.Markup($"[{Theme.Colour}]     {player?.Name,-21}[/]");

		Console.SetCursorPosition(offsetCol, offsetRow + 1);
		AnsiConsole.Markup($"[{Theme.Colour}]     1 2 3 4 5 6 7 8 9 10 [/]");

		Console.SetCursorPosition(offsetCol, offsetRow + 2);
		AnsiConsole.Markup($"[{Theme.Colour}]   ┌─────────────────────┐[/]");

		string empty = string.Join(" ", Enumerable.Repeat($"{Theme.Empty}", boardSize));
		for (int row = 0; row < boardSize; row++) {
			Console.SetCursorPosition(offsetCol, offsetRow + 3 + row);
			AnsiConsole.Markup($"[{Theme.Colour}]{Convert.ToChar(row + 'A'),2} │ [/][{Theme.EmptyColour}]{empty}[/][{Theme.Colour}] │[/]");
		}

		Console.SetCursorPosition(offsetCol, offsetRow + 13);
		AnsiConsole.Markup($"[{Theme.Colour}]   └─────────────────────┘[/]");
	}

	private void DisplayStatus(GameStatus status, string markupMessage = "")
	{
		string message = Theme.GetStatusMessages(status);

		lock (consoleDisplayLock) {
			(int currCol, int currRow) = Console.GetCursorPosition();

			Console.SetCursorPosition(STATUS_COL, _topRow + STATUS_ROW);
			AnsiConsole.Markup($"[{Theme.Colour}]{(new string(' ', CLEAR_WIDTH))}[/]");

			Console.SetCursorPosition(STATUS_COL, _topRow + STATUS_ROW);
			AnsiConsole.Markup($"[{ITheme.GetColour("yellow", Theme.BackgroundColour)}]     {message,-17}[/][{Theme.Colour}]{markupMessage}[/]");
			
			Console.SetCursorPosition(currCol, currRow);
		}
	}

	private void DisplayFinalSummary(IEnumerable<LeaderboardEntry> leaderboard)
	{
		lock (consoleDisplayLock) {
			(int currCol, int currRow) = Console.GetCursorPosition();

			Console.SetCursorPosition(0, _topRow + GAME_HEIGHT - 2);
			Console.WriteLine();
			Console.WriteLine();
			AnsiConsole.MarkupLine($"[{Theme.Colour}]         Results[/]");
			AnsiConsole.MarkupLine($"[{Theme.Colour}]  [bold]Pos Score  Player Name         [/][/]");
			foreach (LeaderboardEntry playerWithScore in leaderboard) {
				AnsiConsole.MarkupLineInterpolated($"[{(playerWithScore.Position == 1 ? $"{Theme.WinnerColour}" : $"{Theme.Colour}")}]   {playerWithScore.Position}   {playerWithScore.Score,3}   {playerWithScore.Name,-20}[/]");
			}
		}
	}

	private void DisplayShotsOnGrid(Player player, int offsetCol = 4)
	{
		int offsetRow = _topRow + BOARD_ROW;
		
		IEnumerable<AttackResult> shots = _attackResults.Where(s => s.TargetedPlayerId == player.Id);

		lock (consoleDisplayLock) {
			(int currCol, int currRow) = Console.GetCursorPosition();

			foreach (AttackResult shot in shots) {
				Console.SetCursorPosition(offsetCol + 3 + (shot.AttackCoordinate.Col * 2), offsetRow + 2 + shot.AttackCoordinate.Row);
				bool sunk = shots.Any(s => s.ShipType == shot.ShipType && s.AttackResultType == AttackResultType.HitAndSunk);
				if (shot.AttackResultType is AttackResultType.Miss or AttackResultType.Hit or AttackResultType.HitAndSunk) {
					ShipType shipType = shot.ShipType ?? ShipType.AircraftCarrier;
					string attackResultString = shot.AttackResultType switch
					{
						AttackResultType.Miss => $"[{Theme.MissColour}]{Theme.Miss}[/]",
						AttackResultType.Hit => sunk ? $"[{Theme.SunkColour}]{Theme.GetShipShape(shipType, true).ToUpper()}[/]" : $"[{Theme.HitColour}]{Theme.GetShipShape(shipType, false)}[/]",
						AttackResultType.HitAndSunk => $"[{Theme.SunkColour}]{Theme.GetShipShape(shipType, true).ToUpper()}[/]",
						_ => throw new ApplicationException($"Not expecting the attack result: {nameof(shot.AttackResultType)}"),
					};
					AnsiConsole.Markup(attackResultString);
				}
			}

			Console.SetCursorPosition(currCol, currRow);
		}
	}

	private void DisplayShipsOnGrid(IEnumerable<Ship>? ships, int offsetCol = 4)
	{
		int offsetRow = _topRow + BOARD_ROW;

		lock (consoleDisplayLock) {
			(int currCol, int currRow) = Console.GetCursorPosition();

			if (ships is not null) {
				foreach (Ship ship in ships) {
					foreach (ShipSegment segment in ship.Segments.Values) {
						string AttackResultType = segment.IsHit
							? $"[{Theme.HitColour}]{Theme.GetShipShape(ship.Type, ship.IsSunk)}[/]"
							: $"[{Theme.Colour}]{Theme.GetShipShape(ship.Type, false)}[/]";
						AttackResultType = ship.IsSunk ? AttackResultType.ToUpper() : AttackResultType;
						Console.SetCursorPosition(offsetCol + 3 + (segment.Coordinate.Col * 2), offsetRow + 2 + segment.Coordinate.Row);
						AnsiConsole.Markup(AttackResultType);
					}
				}
			}

			Console.SetCursorPosition(currCol, currRow);
		}
	}

	private static int PrepareGameSpace()
	{
		for (int i = 0; i < GAME_HEIGHT; i++) {
			Console.WriteLine();
		}

		(_, int bottomRow) = Console.GetCursorPosition();
		return bottomRow - GAME_HEIGHT;
	}

	private static bool TryGetCoordinateFromUser(int inputRow, string colour, out Coordinate coordinate)
	{
		string currentCoordinateString = "";
		coordinate = new(0, 0);

		while (true) {
			ConsoleKey key = DisplayAndGetInput(INPUT_COL, inputRow, CLEAR_WIDTH, colour, $"[bold]{currentCoordinateString}[/]", "     Target coordinates: ");

			if (key is ConsoleKey.Escape) {
				return false;
			} else if (key is ConsoleKey.Enter && currentCoordinateString.Length > 1) {
				coordinate = Coordinate.Parse(currentCoordinateString);
				return true;
			}

			switch (key) {
				case ConsoleKey.Backspace when currentCoordinateString.Length > 0:
					currentCoordinateString = currentCoordinateString[..^1];
					break;
				case >= ConsoleKey.A and <= ConsoleKey.J when currentCoordinateString.Length == 0:
					currentCoordinateString += key;
					break;
				case (>= ConsoleKey.D1 and <= ConsoleKey.D9)
				  or (>= ConsoleKey.NumPad1 and <= ConsoleKey.NumPad9) when currentCoordinateString.Length == 1:
					currentCoordinateString += key.ToString()[^1];
					break;
				case ConsoleKey.D0 or ConsoleKey.NumPad0 when currentCoordinateString.Length == 2 && currentCoordinateString[1] == '1':
					currentCoordinateString += key.ToString()[^1];
					break;
				default:
					break;
			}
		}
	}

	private static (Orientation Orientation, Coordinate Coordinate)? GetShipPlacementFromUser(int inputRow, string colour)
	{
		Coordinate coordinate;
		string currentCoordinateString = "";
		Orientation orientation = Orientation.Horizontal;

		while (true) {
			ConsoleKey key = DisplayAndGetInput(INPUT_COL, inputRow, CLEAR_WIDTH, colour, $"[bold]{currentCoordinateString}[/]", $" Position ({orientation,10}): ");

			if (key is ConsoleKey.Escape) {
				return null;
			} else if (key is ConsoleKey.Enter && currentCoordinateString.Length > 1) {
				coordinate = Coordinate.Parse(currentCoordinateString);
				return (orientation, coordinate);
			}

			switch (key) {
				case ConsoleKey.Backspace when currentCoordinateString.Length > 0:
					currentCoordinateString = currentCoordinateString[..^1];
					break;
				case >= ConsoleKey.A and <= ConsoleKey.J when currentCoordinateString.Length == 0:
					currentCoordinateString += key;
					break;
				case (>= ConsoleKey.D1 and <= ConsoleKey.D9)
				  or (>= ConsoleKey.NumPad1 and <= ConsoleKey.NumPad9) when currentCoordinateString.Length == 1:
					currentCoordinateString += key.ToString()[^1];
					break;
				case ConsoleKey.D0 or ConsoleKey.NumPad0 when currentCoordinateString.Length == 2 && currentCoordinateString[1] == '1':
					currentCoordinateString += key.ToString()[^1];
					break;
				case ConsoleKey.LeftArrow or ConsoleKey.RightArrow:
					orientation = Orientation.Horizontal;
					break;
				case ConsoleKey.UpArrow or ConsoleKey.DownArrow:
					orientation = Orientation.Vertical;
					break;
				default:
					break;
			}
		}
	}

	private static ConsoleKey DisplayAndGetInput(int col, int row, int clearCols, string colour, string input, string message = "Press <Esc> to exit ... ")
	{
		const int ONE_MINUTE = 60000;

		lock (consoleDisplayLock) {
			Console.SetCursorPosition(col, row);
			AnsiConsole.Markup($"[{colour}]{new string(' ', clearCols)}[/]");

			Console.SetCursorPosition(col, row);
			AnsiConsole.Markup($"[{colour}]{message}{input}[/]");
		}

		// If we get a timeout return a key that we don't use (Zoom)
		return KeyReader.ReadKey(ONE_MINUTE) ?? ConsoleKey.Zoom;
	}
}
