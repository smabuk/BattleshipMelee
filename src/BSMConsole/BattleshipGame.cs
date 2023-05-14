using BattleshipEngine;

namespace BSMConsole;
internal class BattleshipGame
{
	private const int OneSecond = 1000;

	private long _timerStart;
	private int _bottomRow;
	private int _topRow = int.MinValue;

	internal void Play()
	{
		Console.WriteLine();
		(int _, _bottomRow) = Console.GetCursorPosition();

		Game game = new Game();
		Dictionary<Coordinate, AttackResult> shots = new();

		GameStatus gameStatus = GameStatus.AddPlayers;

		Player human = game.AddPlayer("Human");
		_ = game.AddPlayer("Computer", isComputer: true);

		DisplayGameUsingSpectre(human);
		(_, int top) = Console.GetCursorPosition();
		int inputRow = top - 5;

		Console.SetCursorPosition(4, inputRow);
		string name = AnsiConsole.Ask<string>("What is your [green]name[/]?", "Human");
		human = human with { Name = name };

		gameStatus = GameStatus.PlacingShips;

		DisplayGameUsingSpectre(human);

		List<Ship> fleet = game.Fleet(human).Where(ship => ship.IsPositioned == false).ToList();

		foreach (Ship ship in fleet) {
			Ship newShip;
			do {
				DisplayGameUsingSpectre(human);
				Console.SetCursorPosition(0, inputRow);
				Orientation orientation = AnsiConsole.Prompt(
					new SelectionPrompt<Orientation>()
					.Title($"Orientation for your {ship.Type}?")
					.PageSize(4)
					.AddChoices(new[] { Orientation.Horizontal, Orientation.Vertical })
					);
				DisplayGameUsingSpectre(human);
				Coordinate coordinate;
				bool isValid = false;
				do {
					Console.SetCursorPosition(0, inputRow);
					string coord = AnsiConsole.Ask<string>($"Position for your {ship.Type}?", ship.Position);
					isValid = !Coordinate.TryParse(coord, null, out coordinate);
				} while (isValid);

				newShip = new(ship.Type, coordinate, orientation);

			} while (!game.PlaceShip(human, newShip));

		}


		bool quit = AnsiConsole.Confirm("Quit?");

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

		void DisplayBoard(Player? player = null)
		{
			//int boardSize = game.BoardSize;
			//Console.WriteLine();
			//Console.WriteLine("     A B C D E F G H I J "); // Display the column labels
			//Console.WriteLine("    ---------------------");
			//for (int i = 0; i < BoardSize; i++) {
			//	Console.Write("{0,2} |", i + 1); // Display the row label
			//	for (int j = 0; j < BoardSize; j++) {
			//		char symbol = Board[i, j];
			//		if (symbol == ShipChar && !IsPlayerShip(i, j)) // Hide the computer's ships
			//		{
			//			symbol = Empty;
			//		}
			//		Console.Write(" {0}", symbol); // Display the symbol
			//	}
			//	Console.WriteLine(" |");
			//}
			//Console.WriteLine("    ---------------------");
			//Console.WriteLine();
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
	}

	private ConsoleKey DisplayAndGetInput(int row, string input)
	{
		Console.SetCursorPosition(0, row);
		Console.Write(new string(' ', Console.WindowWidth - 2));

		//Console.SetCursorPosition(0, row);
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
