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
	private const int BOARD_ROW = 2;
	private const int GAME_HEIGHT = 22;
	private const int GAME_WIDTH = 66;
	private const int STATUS_ROW = 17;
	private const int CLEAR_WIDTH = 56;

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

		gameStatus = GameStatus.PlacingShips;
		DisplayStatus(gameStatus);

		if (RandomShipPlacement) {
			game.PlaceShips(human, doItForMe: true);
			myFleet = game.Fleet(human).ToDictionary(ship => ship.Type, ship => ship);
		} else {
			DisplayGrid(human);
			PlaceShips();
		}

		if (game.AreFleetsReady) {

			gameStatus = GameStatus.Attacking;
			DisplayStatus(gameStatus);
			DisplayBoards(human, opponent);

			while (game.GameOver is false) {
				if (TryGetCoordinateFromUser(_inputRow, out Coordinate coordinate)) {
					shots.TryAdd(coordinate, game.Fire(human, coordinate));
					List<AttackResult> attackResults = game.OtherPlayersFire().ToList();
					DisplayBoards(human, opponent);
				} else {
					gameStatus = GameStatus.Abandoned;
					break;
				}
			}
		}

		gameStatus = game.GameOver ? GameStatus.GameOver : GameStatus.Abandoned;
		DisplayStatus(gameStatus);

		DisplayFinalSummary();


		void DisplayBoards(PrivatePlayer player, Player opponent)
		{
			DisplayGrid(opponent);
			DisplayGridContents(opponent);
			DisplayGrid(player, offsetCol: RIGHT_GRID);
			DisplayGridContents(player, offsetCol: RIGHT_GRID);
		}

		void DisplayGame()
		{
			if (_topRow == int.MinValue) {
				for (int i = 0; i < GAME_HEIGHT; i++) {
					Console.WriteLine();
				}

				(int _, _bottomRow) = Console.GetCursorPosition();
				_topRow = _bottomRow - GAME_HEIGHT;
				(_, _bottomRow) = Console.GetCursorPosition();
				_inputRow = _bottomRow - 4;
			}

			Console.SetCursorPosition(0, _topRow);
			Console.Write($"┌{new string('─', GAME_WIDTH - 2)}┐");
			Console.WriteLine();
			for (int row = 0; row < GAME_HEIGHT - 4; row++) {
				Console.Write($"|{new string(' ', GAME_WIDTH - 2)}|");
				Console.WriteLine();
			}
			Console.Write($"└{new string('─', GAME_WIDTH - 2)}┘");
			Console.WriteLine();

			Console.SetCursorPosition(3, _topRow);
			Console.Write($" T H E   G A M E   O F   B A T T L E S H I P ");
		}

		void DisplayGrid(Player? player = null, int offsetCol = LEFT_GRID)
		{
			const string SEA_COLOUR = "blue";
			const char SEA = '.';
			int boardSize = game.BoardSize;
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
				Console.Write(" |");
			}
			Console.SetCursorPosition(offsetCol, offsetRow + 13);
			Console.Write("   └─────────────────────┘");
		}

		void DisplayGridContents(Player? player = null, int offsetCol = LEFT_GRID)
		{
			const string HIT_COLOUR = "red";
			const string MISS_COLOUR = "blue";
			const string HIT = $"[{HIT_COLOUR}]x[/]";
			const string SUNK = $"[{HIT_COLOUR}]X[/]";
			const string MISS = $"[{MISS_COLOUR}]O[/]";

			int offsetRow = _topRow + BOARD_ROW;

			// Display ships on the board
			if (player is PrivatePlayer) {
				foreach (Ship ship in myFleet.Values) {
					foreach (ShipSegment segment in ship.Segments.Values) {
						Console.SetCursorPosition(offsetCol + 3 + (segment.Coordinate.Col * 2), offsetRow + 2 + segment.Coordinate.Row);
						string hitormiss = segment.IsHit
							? $"[{HIT_COLOUR}]{GetShipShape(ship.Type)}[/]"
							: GetShipShape(ship.Type);
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

		void DisplayFinalSummary()
		{
			Console.SetCursorPosition(0, _inputRow + 1);
			Console.WriteLine();
			Console.WriteLine();
			Console.WriteLine($" Results");
			AnsiConsole.MarkupLineInterpolated($"  [bold]Pos Score  Player Name    [/]");
			List<RankedPlayer> leaderboard = game.LeaderBoard().ToList();
			foreach (RankedPlayer playerWithScore in leaderboard) {
				AnsiConsole.MarkupLineInterpolated($"   [{(playerWithScore.Position == 1 ? "gold1 on black" : "on black")}]{playerWithScore.Position}   {playerWithScore.Score,3}   {playerWithScore.Player.Name,-20}[/]");
			}
		}

		void DisplayStatus(GameStatus status, string markupMessage = "")
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
			Console.SetCursorPosition(8, _topRow + STATUS_ROW);
			Console.Write(new string(' ', CLEAR_WIDTH));
			Console.SetCursorPosition(8, _topRow + STATUS_ROW);
			AnsiConsole.Markup($"[yellow]{message}[/]{markupMessage}");
		}

		void PlaceShips()
		{
			myFleet = game.Fleet(human).ToDictionary(ship => ship.Type);

			DisplayGridContents(human);

			List<Ship> fleet = game.Fleet(human).Where(ship => ship.IsPositioned == false).ToList();

			foreach (Ship ship in fleet) {
				Ship newShip;
				do {
					DisplayGridContents(human);
					DisplayStatus(GameStatus.PlacingShips, $" [green]{ship.Type.ToFriendlyString()}[/] ({ship.NoOfSegments} segments): ");
					(Orientation Orientation, Coordinate Coordinate)? result = GetShipPlacementFromUser(_inputRow);

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
			ConsoleKey key = DisplayAndGetInput(inputRow, currentCoordinateString, "     Target coordinates: ");
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

	private static (Orientation Orientation, Coordinate Coordinate)? GetShipPlacementFromUser(int inputRow)
	{
		string currentCoordinateString = "";
		Orientation orientation = Orientation.Horizontal;

		while (true) {
			ConsoleKey key = DisplayAndGetInput(inputRow, currentCoordinateString, $" Position ({orientation,9}): ");
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
				orientation = Orientation.Horizontal;
			} else if (key is ConsoleKey.UpArrow or ConsoleKey.DownArrow) {
				orientation = Orientation.Vertical;
			}
		}
	}

	private static string GetShipShape(ShipType? shipType) =>
		shipType?.ToString()[0].ToString().ToLowerInvariant() ?? "S";

	private static ConsoleKey DisplayAndGetInput(int row, string input, string message = "Press <Esc> to exit ... ")
	{
		Console.SetCursorPosition(2, row);
		Console.Write(new string(' ', CLEAR_WIDTH));

		Console.SetCursorPosition(2, row);

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
		Abandoned,
	}
}
