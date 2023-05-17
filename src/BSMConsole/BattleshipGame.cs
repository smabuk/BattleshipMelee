using BattleshipEngine;

namespace BSMConsole;
internal class BattleshipGame
{
	public string PlayerName { get; set; } = "Me";
	public bool RandomShipPlacement { get; set; } = false;
	public bool Verbose { get; set; } = false;
	public GameType GameType { get; set; } = GameType.Classic;

	private const int ONE_MINUTE = 60000;
	private const int LEFT_GRID = 4;
	private const int RIGHT_GRID = 34;

	private static readonly Dictionary<ShipType, string> _shipDisplay = new()
		{
			{ ShipType.Battleship, "b" },
			{ ShipType.Cruiser, "c" },
			{ ShipType.AircraftCarrier, "a" },
			{ ShipType.Destroyer, "d" },
			{ ShipType.Submarine, "s" },
			{ ShipType.RomulanBattleBagel, "r" },
		};

	private int _bottomRow;
	private int _topRow = int.MinValue;
	private int _inputRow;

	private readonly Dictionary<Coordinate, AttackResult> shots = new();
	private Dictionary<ShipType, Ship> myFleet = new();
	private PrivatePlayer human = new("Me");
	private Player opponent = new("Computer", IsComputer: true);

	internal void Play()
	{
		Game game = new Game();

		GameStatus gameStatus = GameStatus.AddingPlayers;

		human = game.AddPlayer(PlayerName);
		opponent = Player.PublicPlayer(game.AddPlayer("Computer", isComputer: true));

		DisplayGame();
		_inputRow = _bottomRow - 1;

		DisplayGrid(human);

		gameStatus = GameStatus.PlacingShips;
		DisplayStatus(gameStatus);

		if (RandomShipPlacement) {
			game.PlaceShips(human, doItForMe: true);
			myFleet = game.Fleet(human).ToDictionary(ship => ship.Type, ship => ship);
		} else {
			PlaceShips();
		}

		if (game.AreFleetsReady) {
			DisplayBoards(human, opponent);

			gameStatus = GameStatus.Attacking;
			DisplayStatus(gameStatus);

			while (game.GameOver is false) {
				if (TryGetCoordinateFromUser(_inputRow, out Coordinate coordinate)) {
					shots.TryAdd(coordinate, game.Fire(human, coordinate));
					List<AttackResult> attackResults = game.OtherPlayersFire().ToList();
					DisplayBoards(human, opponent);
				} else {
					break;
				}
			}
		}

		gameStatus = GameStatus.GameOver;
		DisplayStatus(gameStatus);

		Console.SetCursorPosition(0, _inputRow + 1);
		Console.WriteLine();
		Console.WriteLine($" {(game.GameOver ? "GAME OVER" : "Game abandoned")} - Results");
		AnsiConsole.MarkupLineInterpolated($"  [bold]Pos Score  Player Name    [/]");
		List<RankedPlayer> leaderboard = game.LeaderBoard().ToList();
		foreach (RankedPlayer playerWithScore in leaderboard) {
			AnsiConsole.MarkupLineInterpolated($"   [{(playerWithScore.Position == 1 ? "gold1 on black" : "on black")}]{playerWithScore.Position}   {playerWithScore.Score,3}   {playerWithScore.Player.Name,-20}[/]");
		}


		void UpdateBoard(Player? player = null, int offsetCol = LEFT_GRID, int offsetRow = 2)
		{
			const string HIT_COLOUR = "red";
			const string MISS_COLOUR = "blue";
			const string HIT = $"[{HIT_COLOUR}]x[/]";
			const string SUNK = $"[{HIT_COLOUR}]X[/]";
			const string MISS = $"[{MISS_COLOUR}]O[/]";

			offsetRow += _topRow;

			// Display ships on the board
			if (player is PrivatePlayer) {
				foreach (Ship ship in myFleet.Values) {
					foreach (ShipSegment segment in ship.Segments.Values) {
						Console.SetCursorPosition(offsetCol + 3 + (segment.Coordinate.Col * 2), offsetRow + 2 + segment.Coordinate.Row);
						string hitormiss = segment.IsHit
							? $"[{HIT_COLOUR}]{GetShipShape(ship.Type)}[/]"
							: GetShipShape(ship.Type, ship.Orientation);
						hitormiss = ship.IsSunk ? hitormiss.ToUpper() : hitormiss;
						AnsiConsole.Markup(hitormiss);
					}
				}
			} else {
				foreach (AttackResult shot in shots.Values.Where(s => s.TargetedPlayer == player)) {
					Console.SetCursorPosition(offsetCol + 3 + (shot.AttackCoordinate.Col * 2), offsetRow + 2 + shot.AttackCoordinate.Row);
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

		void DisplayGame()
		{
			if (_topRow == int.MinValue) {
				for (int i = 0; i < 22; i++) {
					Console.WriteLine();
				}

				(int _, _topRow) = Console.GetCursorPosition();
				_topRow -= 22;
			}
			(_, _bottomRow) = Console.GetCursorPosition();

			Console.SetCursorPosition(0, _topRow);
			Console.Write($"┌{new string('─', 68 - 4 - 0)}┐");
			Console.WriteLine();
			for (int row = 0; row < 18; row++) {
				Console.Write($"|{new string(' ', 68 - 4 - 0)}|");
				Console.WriteLine();
			}
			Console.Write($"└{new string('─', 68 - 4 - 0)}┘");
			Console.WriteLine();

			Console.SetCursorPosition(3, _topRow);
			Console.Write($" T H E   G A M E   O F   B A T T L E S H I P ");
		}

		void DisplayStatus(GameStatus status, string markupMessage = "") {
			string message = status switch
			{
				GameStatus.PlacingShips => "Place your ships  ",
				GameStatus.AddingPlayers => "Adding players    ",
				GameStatus.Attacking => "Attack those ships",
				GameStatus.GameOver => "GAME OVER         ",
				_ => "                  "
			};
			Console.SetCursorPosition(8, _topRow + 17);
			Console.Write(new string(' ', 40));
			Console.SetCursorPosition(8, _topRow + 17);
			AnsiConsole.Markup($"[yellow]{message}[/]{markupMessage}");
		}

		void DisplayBoards(PrivatePlayer player, Player opponent)
		{
			DisplayGrid(opponent);
			UpdateBoard(opponent);
			DisplayGrid(player, offsetCol: RIGHT_GRID);
			UpdateBoard(player, offsetCol: RIGHT_GRID);
		}

		void DisplayGrid(Player? player = null, int offsetCol = LEFT_GRID, int offsetRow = 2)
		{
			const string SEA_COLOUR = "blue";
			const char SEA = '.';
			int boardSize = game.BoardSize;
			offsetRow += _topRow;

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
				Console.Write(" |");
			}
			Console.SetCursorPosition(offsetCol, offsetRow + 13);
			Console.Write("   └─────────────────────┘");
		}


		void PlaceShips()
		{
			myFleet = game.Fleet(human).ToDictionary(ship => ship.Type);

			UpdateBoard(human);

			List<Ship> fleet = game.Fleet(human).Where(ship => ship.IsPositioned == false).ToList();

			foreach (Ship ship in fleet) {
				Ship newShip;
				do {
					UpdateBoard(human);
					Console.SetCursorPosition(0, _inputRow);
					Console.Write(new string(' ', 50));
					Console.SetCursorPosition(0, _inputRow);
					(Orientation Orientation, Coordinate Coordinate)? result = GetShipPlacementFromUser(_inputRow, ship);

					if (!result.HasValue) {
						return;
					}
					newShip = new(ship.Type, result.Value.Coordinate, result.Value.Orientation);

				} while (!game.PlaceShip(human, newShip));
				myFleet[newShip.Type] = newShip;
			}
		}
	}

	private static bool TryGetCoordinateFromUser(int inputRow, out Coordinate coordinate)
	{
		string currentCoordinateString = "";

		while (true) {
			ConsoleKey key = DisplayAndGetInput(inputRow, currentCoordinateString, "Target coordinates: ");
			if (key == ConsoleKey.Escape) {
				coordinate = default;
				return false;
			} else if (key == ConsoleKey.Enter && currentCoordinateString.Length > 1) {
				coordinate = new Coordinate(currentCoordinateString);
				return true;
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
	}

	private static (Orientation Orientation, Coordinate Coordinate)? GetShipPlacementFromUser(int inputRow, Ship ship)
	{
		string currentCoordinateString = "";
		Orientation orientation = Orientation.Horizontal;

		while (true) {
			ConsoleKey key = DisplayAndGetInput(inputRow, currentCoordinateString, $"Coordinates for {ship.Type} ({ship.NoOfSegments} segments) ({orientation}): ");
			if (key == ConsoleKey.Escape) {
				return null;
			} else if (key == ConsoleKey.Enter && currentCoordinateString.Length > 1) {
				Coordinate coordinate = new Coordinate(currentCoordinateString);
				return (orientation, coordinate);
			} else if (key == ConsoleKey.Backspace && currentCoordinateString.Length > 0) {
				currentCoordinateString = currentCoordinateString[..^1];
			} else if (key >= ConsoleKey.A && key <= ConsoleKey.J && currentCoordinateString.Length == 0) {
				currentCoordinateString += key;
			} else if (key >= ConsoleKey.D1 && key <= ConsoleKey.D9 && currentCoordinateString.Length == 1) {
				currentCoordinateString += key.ToString()[^1];
			} else if (key is ConsoleKey.LeftArrow or ConsoleKey.RightArrow) {
				orientation = Orientation.Horizontal ;
			} else if (key is ConsoleKey.UpArrow or ConsoleKey.DownArrow) {
				orientation = Orientation.Vertical ;
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

	private static ConsoleKey DisplayAndGetInput(int row, string input, string message = "Press <Esc> to exit ... ")
	{
		Console.SetCursorPosition(0, row);
		Console.Write(new string(' ', Console.WindowWidth - 2));

		Console.SetCursorPosition(0, row);

		Console.ResetColor();
		Console.Write($" {message}{input}");

		// If we get a timeout return a key that we don't use (Zoom)
		return KeyReader.ReadKey(ONE_MINUTE) ?? ConsoleKey.Zoom;
	}

	enum GameStatus
	{
		AddingPlayers,
		PlacingShips,
		Attacking,
		GameOver,
	}
}
