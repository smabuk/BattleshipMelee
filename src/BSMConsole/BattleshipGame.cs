using BattleshipEngine;

namespace BSMConsole;
internal class BattleshipGame
{
	public string PlayerName { get; set; } = "Me";
	public bool RandomShipPlacement { get; set; } = false;
	public bool Verbose{ get; set; } = false;
	public GameType GameType{ get; set; } = GameType.Classic;

	private const int OneSecond = 1000;

	//private long _timerStart;
	private int _bottomRow;
	private int _topRow = int.MinValue;
	private Dictionary<Coordinate, AttackResult> shots = new();
	private Dictionary<ShipType, Ship> myFleet = new();
	private PrivatePlayer human = new("Me");
	private Player opponent = new("Computer", IsComputer: true);

	internal void Play()
	{
		bool quit = false;
		Game game = new Game();

		GameStatus gameStatus = GameStatus.AddPlayers;

		//string name = AnsiConsole.Ask<string>("What is your [green]name[/]?", "Human").Trim();
		human = game.AddPlayer(PlayerName);
		opponent = Player.PublicPlayer(game.AddPlayer("Computer", isComputer: true));

		DisplayGame(human, opponent);
		Console.WriteLine();
		(_, _bottomRow) = Console.GetCursorPosition();
		int inputRow = _bottomRow + 5;

		gameStatus = GameStatus.PlacingShips;
		if (RandomShipPlacement) {
			game.PlaceShips(human, doItForMe: true);
			myFleet = game.Fleet(human).ToDictionary(ship => ship.Type, ship => ship);
		} else {
			quit = PlaceShips();
		}

		DisplayGame(human, opponent);

		gameStatus = GameStatus.Attacking;

		Console.SetCursorPosition(0, inputRow + 1);


		//for (int i = 1; i < 11; i++) {
		//	for (int j = 1; j < 11; j++) {
		//		Coordinate guess = new(i, j);
		//		shots.Add(guess, game.Fire(human, guess));
		//	}

		//}

		DisplayGame(human, opponent);

		Console.SetCursorPosition(0, inputRow + 1);




		void DisplayGame(PrivatePlayer player, Player opponent)
		{
			if (_topRow == int.MinValue) {
				for (int i = 0; i < 20; i++) {
					Console.WriteLine();
				}

				(int _, _topRow) = Console.GetCursorPosition();
				_topRow -= 20;
			}

			Console.SetCursorPosition(0, _topRow);
			Console.Write($"┌{new string('─', Console.WindowWidth - 4 - 0)}┐");
			Console.WriteLine();
			for (int row = 0; row < 20; row++) {
				Console.Write($"|{new string(' ', Console.WindowWidth - 4 - 0)}|");
				Console.WriteLine();
			}
			Console.Write($"└{new string('─', Console.WindowWidth - 4 - 0)}┘");
			Console.WriteLine();


			DisplayBoard(player);
			DisplayBoard(opponent, consoleCol: 44);

		}

		void DisplayBoard(Player? player = null, int consoleCol = 4, int consoleRow = 3)
		{
			const string EMPTY = "[blue].[/]";
			const string SHIP = "S";
			const string HIT = "[red]x[/]";
			const string SUNK = "[red]X[/]";
			const string MISS = "O";

			Console.SetCursorPosition(consoleCol, consoleRow);
			AnsiConsole.Markup($"     [green]{player?.Name}[/]");

			Console.SetCursorPosition(consoleCol, consoleRow + 1);
			int boardSize = game.BoardSize;
			Console.SetCursorPosition(consoleCol, consoleRow + 2);
			Console.Write("     1 2 3 4 5 6 7 8 9 10");
			Console.SetCursorPosition(consoleCol, consoleRow + 3);
			Console.Write("   ┌─────────────────────┐");
			for (int row = 0; row < boardSize; row++) {
				Console.SetCursorPosition(consoleCol, consoleRow + 4 + row);
				Console.Write($"{Convert.ToChar(row + 'A'),2} │");
				for (int col = 0; col < boardSize; col++) {
					string symbol = EMPTY;
					AnsiConsole.Markup($" {symbol}");
				}
				Console.Write(" |");
			}
			Console.SetCursorPosition(consoleCol, consoleRow + 14);
			Console.Write("   └─────────────────────┘");

			// Place ships on the board
			if (player is PrivatePlayer) {
				foreach (Ship ship in myFleet.Values) {
					foreach (ShipSegment segment in ship.Segments.Values) {
						Console.SetCursorPosition(consoleCol + 3 + (segment.Coordinate.Col * 2), consoleRow + 3 + segment.Coordinate.Row);
						string hitormiss = segment.IsHit ? HIT : SHIP;
						AnsiConsole.Markup(hitormiss);
					}
				}
			} else {
				foreach (AttackResult shot in shots.Values.Where(s => s.TargetedPlayer == player)) {
					Console.SetCursorPosition(consoleCol + 3 + (shot.AttackCoordinate.Col * 2), consoleRow + 3 + shot.AttackCoordinate.Row);
					bool sunk = shots.Values.Any(s => s.ShipType == shot.ShipType && s.HitOrMiss == AttackResultType.HitAndSunk);
					string hitormiss = shot.HitOrMiss switch
					{
						AttackResultType.Miss => MISS,
						AttackResultType.Hit => sunk ? SUNK : HIT,
						AttackResultType.HitAndSunk => SUNK,
						_ => EMPTY,
					};
					AnsiConsole.Markup(hitormiss);
				}
			}
		}

		bool PlaceShips()
		{
			myFleet = game.Fleet(human).ToDictionary(ship => ship.Type);

			DisplayGame(human, opponent);

			List<Ship> fleet = game.Fleet(human).Where(ship => ship.IsPositioned == false).ToList();

			foreach (Ship ship in fleet) {
				Ship newShip;
				do {
					DisplayGame(human, opponent);
					Console.SetCursorPosition(0, inputRow);
					Orientation orientation = AnsiConsole.Prompt(
						new SelectionPrompt<Orientation>()
						.Title($"Orientation for your {ship.Type}?")
						.PageSize(4)
						.AddChoices(new[] { Orientation.Horizontal, Orientation.Vertical })
						);
					DisplayGame(human, opponent);
					Coordinate coordinate;
					bool isValid = false;
					do {
						Console.SetCursorPosition(0, inputRow);
						string coord = AnsiConsole.Ask<string>($"Position for your {ship.Type}?", ship.Position);
						if (coord.ToUpperInvariant() == "Q") {
							Console.SetCursorPosition(0, inputRow);
							bool sure = AnsiConsole.Confirm("Are you sure?", false);
							if (sure) {
								return true;
							}
						}
						isValid = !Coordinate.TryParse(coord, null, out coordinate);
					} while (isValid);

					newShip = new(ship.Type, coordinate, orientation);

				} while (!game.PlaceShip(human, newShip));
				myFleet[newShip.Type] = newShip;
			}

			return false;
		}
	}

	private ConsoleKey DisplayAndGetInput(int row, string input)
	{
		Console.SetCursorPosition(0, row);
		Console.Write(new string(' ', Console.WindowWidth - 2));

		Console.SetCursorPosition(0, row);
		//Console.Write($"Time remaining: ");
		//if (TimeRemaining < RedZone) {
		//	Console.ForegroundColor = ConsoleColor.Red;
		//}
		//Console.Write($"{TimeRemaining:m':'ss}");

		Console.ResetColor();
		Console.Write($" Press <Esc> to exit... {input}");

		// If we get a timeout return a key that we don't use (Zoom)
		return KeyReader.ReadKey(OneSecond) ?? ConsoleKey.Zoom;
	}

	enum GameStatus
	{
		AddPlayers,
		PlacingShips,
		Attacking,
		GameOver,
	}
}
