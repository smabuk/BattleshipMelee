using BattleshipEngine;

namespace BSMConsole;
internal class BattleshipGame
{
	private const int OneSecond = 1000;

	private long _timerStart;
	private int _bottomRow;
	private int _topRow = int.MinValue;
	private Dictionary<Coordinate, AttackResult> shots = new();
	private Dictionary<ShipType, Ship> myFleet = new();

	internal void Play()
	{
		bool quit = false;
		Game game = new Game();

		GameStatus gameStatus = GameStatus.AddPlayers;

		string name = AnsiConsole.Ask<string>("What is your [green]name[/]?", "Human").Trim();
		PrivatePlayer human = game.AddPlayer(name);
		_ = game.AddPlayer("Computer", isComputer: true);

		DisplayGame(human);
		Console.WriteLine();
		(_, _bottomRow) = Console.GetCursorPosition();
		int inputRow = _bottomRow + 5;

		gameStatus = GameStatus.PlacingShips;
		quit = PlaceShips();

		DisplayGame(human);

		void DisplayGame(Player player)
		{
			if (_topRow == int.MinValue) {
				for (int i = 0; i < 20; i++) {
					Console.WriteLine();
				}

				(int _, _topRow) = Console.GetCursorPosition();
				_topRow -= 20;
			}

			DisplayBoard(player);

		}

		void DisplayBoard(Player? player = null, int consoleCol = 16, int consoleRow = 0)
		{
			const string EMPTY = ".";
			const string SHIP = "S";
			const string HIT = "X";
			const string MISS = "O";

			Console.SetCursorPosition(consoleCol, consoleRow);

			int boardSize = game.BoardSize;
			Console.WriteLine();
			Console.SetCursorPosition(consoleCol, consoleRow + 1);
			Console.WriteLine("     1 2 3 4 5 6 7 8 9 10");
			Console.SetCursorPosition(consoleCol, consoleRow + 2);
			Console.WriteLine("   ┌─────────────────────┐");
			for (int row = 0; row < boardSize; row++) {
				Console.SetCursorPosition(consoleCol, consoleRow + 3 + row);
				Console.Write($"{Convert.ToChar(row + 'A'),2} │");
				for (int col = 0; col < boardSize; col++) {
					string symbol = EMPTY;
					Console.Write($" {symbol}");
				}
				Console.WriteLine(" |");
			}
			Console.SetCursorPosition(consoleCol, consoleRow + 13);
			Console.WriteLine("   └─────────────────────┘");

			// Place ships on the board

			//List<Ship> ships = new();
			//if (player is not null) {
			//	ships = game.Fleet(player ?? new());
			//}
			foreach (Ship ship in myFleet.Values) {
				foreach (ShipSegment segment in ship.Segments.Values) {
					Console.SetCursorPosition(consoleCol + 3 + (segment.Coordinate.Col * 2), consoleRow + 2 + segment.Coordinate.Row);
					string hitormiss = segment.IsHit ? HIT : SHIP;
					Console.Write(hitormiss);
				}
			}
		}


		void DisplayGameUsingSpectre(Player player)
		{
			Layout layout = new Layout("Root")
				.SplitRows(
				new Layout("Top")
					.SplitColumns(
					new Layout("Opponent"),
					new Layout("Player")
					),
				new Layout("Bottom")
					.Size(8)
					.SplitColumns(
					new Layout("Prompt"),
					new Layout("Status")
					)
				);

			layout["Prompt"].Update(
				new Panel(
					Align.Center(new Text(""), VerticalAlignment.Middle))
					.Header(player.Name)
					.NoBorder()
					.Expand()
				);



			Grid playerGrid = new();
			playerGrid.AddColumn();
			playerGrid.AddColumn();
			playerGrid.AddColumn();
			playerGrid.AddColumn();
			playerGrid.AddColumn();
			playerGrid.AddColumn();
			playerGrid.AddColumn();
			playerGrid.AddColumn();
			playerGrid.AddColumn();
			playerGrid.AddColumn();
			playerGrid.AddColumn();

			playerGrid.AddRow("", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10");
			playerGrid.AddRow("A", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".");
			playerGrid.AddRow("B", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".");
			playerGrid.AddRow("C", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".");
			playerGrid.AddRow("D", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".");
			playerGrid.AddRow("E", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".");
			playerGrid.AddRow("F", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".");
			playerGrid.AddRow("G", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".");
			playerGrid.AddRow("H", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".");
			playerGrid.AddRow("I", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".");
			playerGrid.AddRow("J", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".");

			layout["Player"].Update(
				new Panel(
					Align.Center(playerGrid, VerticalAlignment.Middle))
					.Header(player.Name)
					.Expand()
				);

			Grid opponentGrid = new();
			opponentGrid.AddColumn();
			opponentGrid.AddColumn();
			opponentGrid.AddColumn();
			opponentGrid.AddColumn();
			opponentGrid.AddColumn();
			opponentGrid.AddColumn();
			opponentGrid.AddColumn();
			opponentGrid.AddColumn();
			opponentGrid.AddColumn();
			opponentGrid.AddColumn();
			opponentGrid.AddColumn();

			opponentGrid.AddRow("", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10");
			opponentGrid.AddRow("A", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".");
			opponentGrid.AddRow("B", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".");
			opponentGrid.AddRow("C", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".");
			opponentGrid.AddRow("D", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".");
			opponentGrid.AddRow("E", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".");
			opponentGrid.AddRow("F", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".");
			opponentGrid.AddRow("G", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".");
			opponentGrid.AddRow("H", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".");
			opponentGrid.AddRow("I", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".");
			opponentGrid.AddRow("J", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".");

			layout["Opponent"].Update(
				new Panel(
					Align.Center(opponentGrid, VerticalAlignment.Middle))
					.Header(game.OpponentName(player))
					.Expand()
				);

			if (gameStatus == GameStatus.PlacingShips) {

				layout["Status"].Update(
					new Panel(
						Align.Center(new Text(""), VerticalAlignment.Middle))
						.Header("Ship placement")
						.Expand()
					);

			}

			AnsiConsole.Clear();
			AnsiConsole.Write(layout);
		}

		bool PlaceShips()
		{
			myFleet = game.Fleet(human).ToDictionary(ship => ship.Type);

			DisplayGame(human);

			List<Ship> fleet = game.Fleet(human).Where(ship => ship.IsPositioned == false).ToList();

			foreach (Ship ship in fleet) {
				Ship newShip;
				do {
					DisplayGame(human);
					Console.SetCursorPosition(0, inputRow);
					Orientation orientation = AnsiConsole.Prompt(
						new SelectionPrompt<Orientation>()
						.Title($"Orientation for your {ship.Type}?")
						.PageSize(4)
						.AddChoices(new[] { Orientation.Horizontal, Orientation.Vertical })
						);
					DisplayGame(human);
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
