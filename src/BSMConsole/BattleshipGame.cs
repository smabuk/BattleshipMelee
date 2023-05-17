using BattleshipEngine;

namespace BSMConsole;
internal class BattleshipGame
{
	public string PlayerName { get; set; } = "Me";
	public bool RandomShipPlacement { get; set; } = false;
	public bool Verbose{ get; set; } = false;
	public GameType GameType{ get; set; } = GameType.Classic;

	private const int OneMinute = 60000;

	private int _bottomRow;
	private int _topRow = int.MinValue;
	private Dictionary<Coordinate, AttackResult> shots = new();
	private Dictionary<ShipType, Ship> myFleet = new();
	private PrivatePlayer human = new("Me");
	private Player opponent = new("Computer", IsComputer: true);
	private readonly Dictionary<ShipType, string> _shipDisplay = new()
		{
			{ ShipType.Battleship, "b" },
			{ ShipType.Cruiser, "c" },
			{ ShipType.AircraftCarrier, "a" },
			{ ShipType.Destroyer, "d" },
			{ ShipType.Submarine, "s" },
			{ ShipType.RomulanBattleBagel, "r" },
		};

	internal void Play()
	{

		Game game = new Game();

		GameStatus gameStatus = GameStatus.AddingPlayers;

		human = game.AddPlayer(PlayerName);
		opponent = Player.PublicPlayer(game.AddPlayer("Computer", isComputer: true));

		DisplayGame(human, opponent);
		Console.WriteLine();
		(_, _bottomRow) = Console.GetCursorPosition();
		int inputRow = _bottomRow + 5;

		gameStatus = GameStatus.PlacingShips;
		DisplayStatus(gameStatus);

		if (RandomShipPlacement) {
			game.PlaceShips(human, doItForMe: true);
			myFleet = game.Fleet(human).ToDictionary(ship => ship.Type, ship => ship);
		} else {
			PlaceShips();
		}

		DisplayBoards(human, opponent);

		gameStatus = GameStatus.Attacking;
		DisplayStatus(gameStatus);

		Console.SetCursorPosition(0, inputRow + 1);

		// Attack all of the spaces to clear the board
		//for (int i = 1; i < 11; i++) {
		//	for (int j = 1; j < 11; j++) {
		//		Coordinate guess = new(i, j);
		//		shots.Add(guess, game.Fire(human, guess));
		//		List<AttackResult> attackResults = game.OtherPlayersFire().ToList();
		//	}
		//}


		DisplayBoards(human, opponent);
		
		string currentCoordinateString = "";
		while (game.GameOver is false) {

			ConsoleKey key = DisplayAndGetInput(inputRow, currentCoordinateString);
			if (key == ConsoleKey.Escape) {
				break;
			} else if (key == ConsoleKey.Enter && currentCoordinateString.Length > 1) {
				shots.TryAdd(currentCoordinateString, game.Fire(human, currentCoordinateString));
				List<AttackResult> attackResults = game.OtherPlayersFire().ToList();
				currentCoordinateString = "";
				DisplayBoards(human, opponent);
			} else if (key == ConsoleKey.Backspace && currentCoordinateString.Length > 0) {
				currentCoordinateString = currentCoordinateString[..^1];
			} else if (key >= ConsoleKey.A && key <= ConsoleKey.J && currentCoordinateString.Length == 0) {
				currentCoordinateString += key;
			} else if (key >= ConsoleKey.D1 && key <= ConsoleKey.D9 && currentCoordinateString.Length == 1) {
				currentCoordinateString += key.ToString()[^1];
			} else if (key == ConsoleKey.D0 && currentCoordinateString[1] == '1') {
				currentCoordinateString += key.ToString()[^1];
			}
		}

		gameStatus = GameStatus.GameOver;
		DisplayStatus(gameStatus);
		
		Console.SetCursorPosition(0, inputRow + 1);
		Console.WriteLine("Results");
		List<PlayerWithScore> leaderboard = game.LeaderBoard().ToList();
		foreach (PlayerWithScore playerWithScore in leaderboard) {
			AnsiConsole.MarkupLineInterpolated($"   [{(playerWithScore.Position == 1 ? "gold1 on black" : "white on black")}]{playerWithScore.Position} {playerWithScore.Score, 3}  {playerWithScore.Player.Name, -20} [/]");
		}



		void DisplayGame(PrivatePlayer player, Player opponent)
		{
			Console.Clear();
			if (_topRow == int.MinValue) {
				for (int i = 0; i < 20; i++) {
					Console.WriteLine();
				}

				(int _, _topRow) = Console.GetCursorPosition();
				_topRow -= 20;
			}

			Console.SetCursorPosition(0, _topRow);
			Console.Write($"┌{new string('─', 68 - 4 - 0)}┐"); 
			Console.WriteLine();
			for (int row = 0; row < 20; row++) {
				Console.Write($"|{new string(' ', 68 - 4 - 0)}|");
				Console.WriteLine();
			}
			Console.Write($"└{new string('─', 68 - 4 - 0)}┘");
			Console.WriteLine();

			Console.SetCursorPosition(3, _topRow);
			Console.Write($"T H E   G A M E   O F   B A T T L E S H I P");

			DisplayBoards(player, opponent);
		}

		void DisplayStatus(GameStatus status) {
			Console.SetCursorPosition(7, _topRow + 2);
			string message = status switch
			{
				GameStatus.PlacingShips => "Place your ships  ",
				GameStatus.AddingPlayers   => "Adding players    ",
				GameStatus.Attacking    => "Attack those ships",
				GameStatus.GameOver     => "GAME OVER         ",
				_                       => "                  "
			};
			AnsiConsole.Markup($"[yellow]{message}[/]{new string(' ', 30)}");  // did use Console.WindowWidth

		}

		void DisplayBoards(PrivatePlayer player, Player opponent)
		{
			DisplayGrid(player);
			UpdateBoard(player);
			DisplayGrid(opponent, consoleCol: 34);
			UpdateBoard(opponent, consoleCol: 34);
		}

		void UpdateBoard(Player? player = null, int consoleCol = 4, int consoleRow = 4, bool noGridOrSea = false)
		{
			const string HIT_COLOUR = "red";
			const string MISS_COLOUR = "blue";
			const string HIT = $"[{HIT_COLOUR}]x[/]";
			const string SUNK = $"[{HIT_COLOUR}]X[/]";
			const string MISS = $"[{MISS_COLOUR}]O[/]";

			// Place ships on the board
			if (player is PrivatePlayer) {
				foreach (Ship ship in myFleet.Values) {
					foreach (ShipSegment segment in ship.Segments.Values) {
						Console.SetCursorPosition(consoleCol + 3 + (segment.Coordinate.Col * 2), consoleRow + 2 + segment.Coordinate.Row);
						string hitormiss = segment.IsHit ? $"[{HIT_COLOUR}]{GetShipShape(ship.Type)}[/]" : GetShipShape(ship.Type, ship.Orientation);
						hitormiss = ship.IsSunk ? hitormiss.ToUpper() : hitormiss;
						AnsiConsole.Markup(hitormiss);
					}
				}
			} else {
				foreach (AttackResult shot in shots.Values.Where(s => s.TargetedPlayer == player)) {
					Console.SetCursorPosition(consoleCol + 3 + (shot.AttackCoordinate.Col * 2), consoleRow + 2 + shot.AttackCoordinate.Row);
					bool sunk = shots.Values.Any(s => s.ShipType == shot.ShipType && s.HitOrMiss == AttackResultType.HitAndSunk);
					string hitormiss = shot.HitOrMiss switch
					{
						AttackResultType.Miss => MISS,
						AttackResultType.Hit => sunk ? $"[{HIT_COLOUR}]{GetShipShape(shot.ShipType).ToUpper()}[/]" : $"[{HIT_COLOUR}]{GetShipShape(shot.ShipType)}[/]",
						AttackResultType.HitAndSunk => $"[{HIT_COLOUR}]{GetShipShape(shot.ShipType).ToUpper()}[/]",
						AttackResultType.AlreadyAttacked => throw new ArgumentOutOfRangeException(nameof(shot.HitOrMiss)),
						AttackResultType.InvalidPosition => throw new ArgumentOutOfRangeException(nameof(shot.HitOrMiss)),
						_ => throw new ArgumentOutOfRangeException(nameof(shot.HitOrMiss)),
					};
					AnsiConsole.Markup(hitormiss);
				}
			}
		}

		void DisplayGrid(Player? player = null, int consoleCol = 4, int consoleRow = 4)
		{
			const string SEA_COLOUR = "blue";
			const string SEA = $"[{SEA_COLOUR}].[/]";
			int boardSize = game.BoardSize;

			Console.SetCursorPosition(consoleCol, consoleRow);
			AnsiConsole.Markup($"     [green]{player?.Name}[/]");

			Console.SetCursorPosition(consoleCol, consoleRow + 1);
			Console.Write("     1 2 3 4 5 6 7 8 9 10");
			Console.SetCursorPosition(consoleCol, consoleRow + 2);
			Console.Write("   ┌─────────────────────┐");
			for (int row = 0; row < boardSize; row++) {
				Console.SetCursorPosition(consoleCol, consoleRow + 3 + row);
				Console.Write($"{Convert.ToChar(row + 'A'),2} │");
				for (int col = 0; col < boardSize; col++) {
					string symbol = SEA;
					AnsiConsole.Markup($" {symbol}");
				}
				Console.Write(" |");
			}
			Console.SetCursorPosition(consoleCol, consoleRow + 13);
			Console.Write("   └─────────────────────┘");
		}

		void PlaceShips()
		{
			myFleet = game.Fleet(human).ToDictionary(ship => ship.Type);

			DisplayBoards(human, opponent);

			List<Ship> fleet = game.Fleet(human).Where(ship => ship.IsPositioned == false).ToList();

			foreach (Ship ship in fleet) {
				Ship newShip;
				do {
					UpdateBoard(human);
					Console.SetCursorPosition(0, inputRow);
					Console.Write(new string(' ', 50));
					Console.SetCursorPosition(0, inputRow);
					Orientation orientation = AnsiConsole.Prompt(
					new SelectionPrompt<Orientation>()
						.Title($"Orientation for your {ship.Type} ({ship.NoOfSegments} segments)?")
						.PageSize(4)
						.AddChoices(new[] { Orientation.Horizontal, Orientation.Vertical })
						);
					UpdateBoard(human);
					Coordinate coordinate;
					bool isValid = false;
					do {
						Console.SetCursorPosition(0, inputRow);
						Console.Write(new string(' ', 50));
						Console.SetCursorPosition(0, inputRow);
						string coord = AnsiConsole.Ask<string>($"Position for your {ship.Type} ({ship.NoOfSegments} segments)?");
						isValid = !Coordinate.TryParse(coord, null, out coordinate);
					} while (isValid);

					newShip = new(ship.Type, coordinate, orientation);

				} while (!game.PlaceShip(human, newShip));
				myFleet[newShip.Type] = newShip;
			}
		}
	}

	private string GetShipShape(ShipType? shipType, Orientation orientation = Orientation.Horizontal)
	{
		if (shipType is null) {
			return "S";
		}
		return _shipDisplay[(ShipType)shipType];
	}

	private static ConsoleKey DisplayAndGetInput(int row, string input)
	{
		Console.SetCursorPosition(0, row);
		Console.Write(new string(' ', Console.WindowWidth - 2));

		Console.SetCursorPosition(0, row);

		Console.ResetColor();
		Console.Write($" Enter a coordinate then Enter, or press <Esc> to exit... {input}");

		// If we get a timeout return a key that we don't use (Zoom)
		return KeyReader.ReadKey(OneMinute) ?? ConsoleKey.Zoom;
	}

	enum GameStatus
	{
		AddingPlayers,
		PlacingShips,
		Attacking,
		GameOver,
	}
}
