//BattleShipsEngine.Execute();

using BattleshipEngine;


Game game = new Game();
Dictionary<Coordinate, AttackResult> shots = new();

GameStatus gameStatus = GameStatus.AddPlayers;

Player human = game.AddPlayer("Human");
_ = game.AddPlayer("Computer", isComputer: true);

DisplayGame(human);
(_, int top) = Console.GetCursorPosition();
int inputRow = top - 5;

Console.SetCursorPosition(4, inputRow);
string name = AnsiConsole.Ask<string>("What is your [green]name[/]?", "Human");
human = human with { Name = name };

gameStatus = GameStatus.PlacingShips;

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
			isValid = !Coordinate.TryParse(coord, null, out coordinate);
		} while (isValid);

		newShip = new(ship.Type, coordinate, orientation);

	} while (!game.PlaceShip(human, newShip));

}


bool quit = AnsiConsole.Confirm("Quit?");


void DisplayGame(Player player)
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

enum GameStatus
{
	AddPlayers,
	PlacingShips,
	Attacking,
	GameOver,
}
