using BattleshipEngine;

namespace BSMConsole;

internal class BattleshipGame
{
	public string PlayerName { get; set; } = "Me";
	public bool RandomShipPlacement { get; set; } = false;
	public GameType GameType { get; set; } = GameType.Classic;
	public bool NetworkPlay = false;
	public string Uri = "";

	// Game layout constants
	private const int LEFT_GRID = 4;
	private const int RIGHT_GRID = 34;
	private const int BOARD_ROW = 2;
	private const int GAME_HEIGHT = 22;
	private const int GAME_WIDTH = 66;
	private const int STATUS_ROW = 17;
	private const int INPUT_COL = 2;
	private const int INPUT_ROW = 18;
	private const int CLEAR_WIDTH = 56;


	static object consoleDisplayLock = new();
	private int _topRow = int.MinValue;

	private HubConnection _hubConnection = default!;
	
	private readonly List<AttackResult> _attackResults = new();
	private Dictionary<ShipType, Ship> _myFleet = new();

	private AuthPlayer player = new("Me");
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

		if (RandomShipPlacement) {
			game.PlaceShips(player, doItForMe: true);
			_myFleet = game.Fleet(player).ToDictionary(ship => ship.Type, ship => ship);
		} else {
			DisplayEmptyGrid(player, game.BoardSize);
			PlaceShips(game);
		}

		if (game.AreFleetsReady) {

			gameStatus = GameStatus.Attacking;
			DisplayStatus(gameStatus);
			DisplayBoards(player, opponent, _myFleet.Values, game.BoardSize);

			while (game.GameOver is false) {
				if (TryGetCoordinateFromUser(_topRow + INPUT_ROW, out Coordinate coordinate)) {
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

		DisplayFinalSummary(game);
	}

	internal async Task PlayNetworkGame()
	{
		Game game = new Game();
		GameId gameId;
		GameStatus gameStatus = GameStatus.AddingPlayers;

		await PrepareNetwork(Uri);

		//_hubConnection.On<Player>("Opponents", (player) => {
		//	Debug.WriteLine($"Opponent: {player}");
		//	opponent = player is ComputerPlayer p ? p : throw new ApplicationException("No opponents found.");
		//});
		
		player = await RegisterPlayer(PlayerName);

		_topRow = PrepareGameSpace();

		DisplayGame();

		_hubConnection.On<GameId>("StartGame", (gId) => { gameId = gId; });
		_hubConnection.On<GameStatus>("GameStatusChange", (gStat) => { gameStatus = gStat; DisplayStatus(gameStatus); });

		
		gameId = await PlayVsComputer(player, gameType: GameType);

		opponent = await _hubConnection.InvokeAsync<ComputerPlayer>("FindComputerOpponent", player, gameId);

		List<Ship> ships = Game.GameShips(GameType);
		if (RandomShipPlacement is false) {
			DisplayEmptyGrid(player, game.BoardSize);
			ships = PlaceShipsForNetworkPlay(ships);
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
				_hubConnection
				.InvokeAsync<List<Ship>>("PlaceShips", player, gameId, ships, RandomShipPlacement))
			.ToDictionary(ship => ship.Type, ship => ship);

		Task.Delay(500).Wait();
		
		_hubConnection.On<List<AttackResult>>("AttackResults", (attackResults) => {
			foreach (AttackResult attackResult in attackResults.Where(ar => ar.TargetedPlayerId == player.Id)) {
				if (attackResult.ShipType is ShipType s) {
					_myFleet[s].Attack(attackResult.AttackCoordinate);
				}
			}
			_attackResults.AddRange(attackResults);
			DisplayShotsOnGrid(opponent);
			DisplayShotsOnGrid(player, RIGHT_GRID);
			//Console.SetCursorPosition(INPUT_COL, _topRow + INPUT_ROW);
		});

		if (gameStatus is GameStatus.Attacking) {

			gameStatus = GameStatus.Attacking;
			DisplayStatus(gameStatus);
			DisplayBoards(player, opponent, _myFleet.Values, game.BoardSize);

			while (gameStatus is GameStatus.Attacking) {
				if (TryGetCoordinateFromUser(_topRow + INPUT_ROW, out Coordinate coordinate)) {
					List<AttackResult> attackResults = await _hubConnection
						.InvokeAsync<List<AttackResult>>("Fire", player, gameId, coordinate);
					//DisplayBoards(player, opponent, myFleet.Values, game.BoardSize);
					//DisplayShotsOnGrid(opponent);
					//DisplayShotsOnGrid(player, RIGHT_GRID);
				} else {
					gameStatus = GameStatus.Abandoned;
					break;
				}
			}
		}

		DisplayStatus(gameStatus);

		Console.SetCursorPosition(0, _topRow + GAME_HEIGHT - 2);
		Console.WriteLine();
		Console.WriteLine();
		Console.WriteLine($" Results");
		AnsiConsole.MarkupLineInterpolated($"  [bold]Pos Score  Player Name    [/]");
		List<LeaderboardEntry> leaderboard = await _hubConnection
			.InvokeAsync<List<LeaderboardEntry>>("Leaderboard", gameId);
		foreach (LeaderboardEntry playerWithScore in leaderboard) {
			AnsiConsole.MarkupLineInterpolated($"   [{(playerWithScore.Position == 1 ? "gold1 on black" : "on black")}]{playerWithScore.Position}   {playerWithScore.Score,3}   {playerWithScore.Name,-20}[/]");
		}
	}

	private void PlaceShips(Game game)
	{
		_myFleet = Game.GameShips(GameType).ToDictionary(ship => ship.Type);

		DisplayShipsOnGrid(_myFleet.Values);

		List<Ship> fleet = _myFleet.Values.Where(ship => ship.IsPositioned == false).ToList();

		foreach (Ship ship in fleet) {
			Ship newShip;
			do {
				DisplayShipsOnGrid(_myFleet.Values);
				DisplayStatus(GameStatus.PlacingShips, $" [green]{ship.Type.ToFriendlyString()}[/] ({ship.NoOfSegments} segments) ");
				(Orientation Orientation, Coordinate Coordinate)? result = GetShipPlacementFromUser(_topRow + INPUT_ROW);


				if (!result.HasValue) {
					return;
				}
				newShip = new(ship.Type, result.Value.Coordinate, result.Value.Orientation);

			} while (!game.PlaceShip(player, newShip));
			_myFleet[newShip.Type] = newShip;
		}
	}

	private List<Ship> PlaceShipsForNetworkPlay(IEnumerable<Ship> myFleet)
	{
		List<Ship> fleet = myFleet.ToList();
		List<Ship> newFleet = myFleet.ToList();

		for (int i = 0; i < fleet.Count(); i++) {
			Ship ship = fleet[i];
			bool badPlacement = true;
			do {
				DisplayShipsOnGrid(fleet);
				DisplayStatus(GameStatus.PlacingShips, $" [green]{ship.Type.ToFriendlyString()}[/] ({ship.NoOfSegments} segments) ");
				(Orientation Orientation, Coordinate Coordinate)? result = GetShipPlacementFromUser(_topRow + INPUT_ROW);

				if (!result.HasValue) {
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
		Console.SetCursorPosition(0, _topRow);
		Console.Write($"┌{new string('─', GAME_WIDTH - 2)}┐");
		Console.WriteLine();
		for (int row = 0; row < GAME_HEIGHT - 4; row++) {
			Console.Write($"│{new string(' ', GAME_WIDTH - 2)}│");
			Console.WriteLine();
		}
		Console.Write($"└{new string('─', GAME_WIDTH - 2)}┘");
		Console.WriteLine();

		Console.SetCursorPosition(3, _topRow);
		Console.Write($" T H E   G A M E   O F   B A T T L E S H I P ");
	}

	private void DisplayEmptyGrid(Player player, int boardSize, int offsetCol = 4)
	{
		const string SEA_COLOUR = "blue on black";
		const char SEA = '.';
		//int boardSize = game.BoardSize;
		int offsetRow = _topRow + BOARD_ROW;

		Console.SetCursorPosition(offsetCol, offsetRow);
		AnsiConsole.Markup($"     [green]{player?.Name}[/]");

		Console.SetCursorPosition(offsetCol, offsetRow + 1);
		Console.Write("     1 2 3 4 5 6 7 8 9 10");
		Console.SetCursorPosition(offsetCol, offsetRow + 2);
		Console.Write("   ┌─────────────────────┐");
		for (int row = 0; row < boardSize; row++) {
			Console.SetCursorPosition(offsetCol, offsetRow + 3 + row);
			Console.Write($"{Convert.ToChar(row + 'A'),2} │ ");
			string sea = string.Join(" ", Enumerable.Repeat($"{SEA}", boardSize));
			AnsiConsole.Markup($"[{SEA_COLOUR}]{sea}[/]");
			Console.Write(" │");
		}
		Console.SetCursorPosition(offsetCol, offsetRow + 13);
		Console.Write("   └─────────────────────┘");
	}

	private void DisplayStatus(GameStatus status, string markupMessage = "")
	{
		string message = status switch
		{
			GameStatus.PlacingShips => "Place your ships  ",
			GameStatus.AddingPlayers => "Adding players    ",
			GameStatus.Attacking => "Attack those ships",
			GameStatus.GameOver => "GAME OVER         ",
			GameStatus.Abandoned => "Abandoned",
			_ => "                  "
		};

		lock (consoleDisplayLock) {
			(int currCol, int currRow) = Console.GetCursorPosition();

			Console.SetCursorPosition(8, _topRow + STATUS_ROW);
			Console.Write(new string(' ', CLEAR_WIDTH));
			Console.SetCursorPosition(8, _topRow + STATUS_ROW);
			AnsiConsole.Markup($"[yellow]{message}[/]{markupMessage}");
			
			Console.SetCursorPosition(currCol, currRow);
		}
	}

	private void DisplayFinalSummary(Game game)
	{
		lock (consoleDisplayLock) {
			(int currCol, int currRow) = Console.GetCursorPosition();

			Console.SetCursorPosition(0, _topRow + GAME_HEIGHT - 2);
			Console.WriteLine();
			Console.WriteLine();
			Console.WriteLine($" Results");
			AnsiConsole.MarkupLineInterpolated($"  [bold]Pos Score  Player Name    [/]");
			List<LeaderboardEntry> leaderboard = game.LeaderBoard().ToList();
			foreach (LeaderboardEntry playerWithScore in leaderboard) {
				AnsiConsole.MarkupLineInterpolated($"   [{(playerWithScore.Position == 1 ? "gold1 on black" : "on black")}]{playerWithScore.Position}   {playerWithScore.Score,3}   {playerWithScore.Name,-20}[/]");
			}

			Console.SetCursorPosition(currCol, currRow);
		}
	}

	private void DisplayShotsOnGrid(Player player, int offsetCol = 4)
	{
		const string HIT_COLOUR = "red on black";
		const string SUNK_COLOUR = $"red on black";
		const string MISS_COLOUR = "blue on black";
		const string MISS = $"[{MISS_COLOUR}]O[/]";

		int offsetRow = _topRow + BOARD_ROW;
		IEnumerable<AttackResult> shots = _attackResults.Where(s => s.TargetedPlayerId == player.Id);

		lock (consoleDisplayLock) {
			(int currCol, int currRow) = Console.GetCursorPosition();

			foreach (AttackResult shot in shots) {
				Console.SetCursorPosition(offsetCol + 3 + (shot.AttackCoordinate.Col * 2), offsetRow + 2 + shot.AttackCoordinate.Row);
				bool sunk = shots.Any(s => s.ShipType == shot.ShipType && s.HitOrMiss == AttackResultType.HitAndSunk);
				if (shot.HitOrMiss is AttackResultType.Miss or AttackResultType.Hit or AttackResultType.HitAndSunk) {
					string hitormiss = shot.HitOrMiss switch
					{
						AttackResultType.Miss => MISS,
						AttackResultType.Hit => sunk ? $"[{SUNK_COLOUR}]{GetShipShape(shot.ShipType).ToUpper()}[/]" : $"[{HIT_COLOUR}]{GetShipShape(shot.ShipType)}[/]",
						AttackResultType.HitAndSunk => $"[{SUNK_COLOUR}]{GetShipShape(shot.ShipType).ToUpper()}[/]",
						AttackResultType.AlreadyAttacked => throw new ArgumentOutOfRangeException(nameof(shot.HitOrMiss)),
						AttackResultType.InvalidPosition => throw new ArgumentOutOfRangeException(nameof(shot.HitOrMiss)),
						_ => throw new ArgumentOutOfRangeException(nameof(shot.HitOrMiss)),
					};
					AnsiConsole.Markup(hitormiss);
				}
			}

			Console.SetCursorPosition(currCol, currRow);
		}
	}

	private void DisplayShipsOnGrid(IEnumerable<Ship>? ships, int offsetCol = 4)
	{
		const string HIT_COLOUR = "red";
		const string HIT = $"[{HIT_COLOUR}]x[/]";
		const string MISS_COLOUR = "blue";
		const string MISS = $"[{MISS_COLOUR}]O[/]";

		int offsetRow = _topRow + BOARD_ROW;

		// Display ships on the board
		lock (consoleDisplayLock) {
			(int currCol, int currRow) = Console.GetCursorPosition();

			if (ships is not null) {
				foreach (Ship ship in ships) {
					foreach (ShipSegment segment in ship.Segments.Values) {
						string hitormiss = segment.IsHit
							? $"[{HIT_COLOUR}]{GetShipShape(ship.Type)}[/]"
							: GetShipShape(ship.Type);
						hitormiss = ship.IsSunk ? hitormiss.ToUpper() : hitormiss;
						Console.SetCursorPosition(offsetCol + 3 + (segment.Coordinate.Col * 2), offsetRow + 2 + segment.Coordinate.Row);
						AnsiConsole.Markup(hitormiss);
					}
				}
			}

			Console.SetCursorPosition(currCol, currRow);
		}
	}

	private async  Task PrepareNetwork(string uri)
	{
		_hubConnection = new HubConnectionBuilder()
			.WithUrl($"{uri}/bsm")
			.WithAutomaticReconnect()
			.Build();

		await _hubConnection.StartAsync();
	}

	private async Task<AuthPlayer> RegisterPlayer(string playerName)
	{
		AuthPlayer privatePlayer;
		try {
			privatePlayer = await _hubConnection.InvokeAsync<AuthPlayer>("RegisterPlayer", playerName);
		}
		catch (Exception ex) {
			Debug.WriteLine($"Error in {nameof(RegisterPlayer)}: {ex.Message}");
			throw;
		}

		Debug.WriteLine($"  Player returned: {player}");
		return privatePlayer;
	}

	private async Task<Player> FindOpponent(bool isComputer = true)
	{
		Player player;
		try {
			player = await _hubConnection.InvokeAsync<ComputerPlayer>("FindComputerOpponent");
		}
		catch (Exception ex) {
			Debug.WriteLine($"Error in {nameof(FindOpponent)}: {ex.Message}");
			throw;
		}

		Debug.WriteLine($"Opponent returned: {player}");
		return player;
	}

	private async Task<GameId> PlayVsComputer(AuthPlayer player, string computerPlayerName = "Computer", GameType gameType = GameType.Classic)
	{
		GameId? gameId;
		try {
			gameId = await _hubConnection.InvokeAsync<GameId>("StartGameVsComputer", player, computerPlayerName, gameType);
		}
		catch (Exception ex) {
			Debug.WriteLine($"Error in {nameof(PlayVsComputer)}: {ex.Message}");
			throw;
		}

		if (gameId is null) {
			throw new ApplicationException($"{nameof(PlayVsComputer)}: Couldn't start a game against the computer.");
		}

		Debug.WriteLine($"Game Id: {gameId}");
		return gameId;
	}

	private static int PrepareGameSpace()
	{
		for (int i = 0; i < GAME_HEIGHT; i++) {
			Console.WriteLine();
		}

		(_, int bottomRow) = Console.GetCursorPosition();
		return bottomRow - GAME_HEIGHT;
	}

	private static bool TryGetCoordinateFromUser(int inputRow, out Coordinate coordinate)
	{
		string currentCoordinateString = "";

		while (true) {
			ConsoleKey key = DisplayAndGetInput(INPUT_COL, inputRow, CLEAR_WIDTH, $"[bold]{currentCoordinateString}[/]", "     Target coordinates: ");
			switch (key) {
				case ConsoleKey.Escape:
					coordinate = new(0, 0);
					return false;
				case ConsoleKey.Enter when currentCoordinateString.Length > 1:
					coordinate = currentCoordinateString;
					return true;

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

	private static (Orientation Orientation, Coordinate Coordinate)? GetShipPlacementFromUser(int inputRow)
	{
		string currentCoordinateString = "";
		Orientation orientation = Orientation.Horizontal;

		while (true) {
			ConsoleKey key = DisplayAndGetInput(INPUT_COL, inputRow, CLEAR_WIDTH, $"[bold]{currentCoordinateString}[/]", $" Position ({orientation,9}): ");
			switch (key) {
				case ConsoleKey.Escape:
					return null;
				case ConsoleKey.Enter when currentCoordinateString.Length > 1: {
						Coordinate coordinate = currentCoordinateString;
						return (orientation, coordinate);
					}

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

	private static string GetShipShape(ShipType? shipType) =>
		shipType?.ToString()[0].ToString().ToLowerInvariant() ?? "S";

	private static ConsoleKey DisplayAndGetInput(int col, int row, int clearCols, string input, string message = "Press <Esc> to exit ... ")
	{
		const int ONE_MINUTE = 60000;

		lock (consoleDisplayLock) {
			Console.SetCursorPosition(col, row);
			Console.Write(new string(' ', clearCols));

			Console.SetCursorPosition(col, row);
			AnsiConsole.Markup($" {message}{input}");
		}

		// If we get a timeout return a key that we don't use (Zoom)
		return KeyReader.ReadKey(ONE_MINUTE) ?? ConsoleKey.Zoom;
	}
}
