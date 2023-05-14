namespace BattleshipEngine;

public record Game(GameType GameType = GameType.Classic)
{
	private readonly Dictionary<Guid, Player> players = new();
	private readonly Dictionary<Guid, Board> boards = new();
	private readonly Dictionary<Guid, List<Coordinate>> shots = new();

	public void Init()
	{
		foreach (Player player in players.Values) {
			boards[player.Id].Init();
			shots[player.Id] = new();
		}
	}

	public bool AreFleetsReady => boards.Values.All(board => board.IsFleetReady);
	public bool GameOver => boards.Values.Any(board => board.IsFleetSunk);
	public List<Ship> Fleet(Player player) => boards[player.Id].Fleet.ToList();
	public string OpponentName(Player player) => Opponent(player).Name;
	private Player Opponent(Player player) => players.Values.Single(p => p.Id != player.Id);

	public Player AddPlayer(string name, bool isComputer = false)
	{
		Player player = new(name, isComputer);

		players.Add(player.Id, player);
		boards.Add(player.Id, new(GameType));

		boards[player.Id].Init();
		shots[player.Id] = new();

		if (isComputer) {
			PlaceShips(player);
		}

		return player;
	}

	public AttackResult Fire(Player player, Coordinate attackCoordinate)
	{
		Player playerToAttack = Opponent(player);

		if (shots[player.Id].Contains(attackCoordinate)) {
			return new(attackCoordinate, AttackResultType.AlreadyAttacked);
		}
		shots[player.Id].Add(attackCoordinate);
		AttackResult result = boards[playerToAttack.Id].Attack(attackCoordinate);
		return result;
	}

	public List<PlayerFinishingPosition> LeaderBoard(Player player)
	{
		if (GameOver) {
			return new()
			{
				new(boards[Opponent(player).Id].IsFleetSunk ? 1 : 2, player.Name),
				new(boards[player.Id].IsFleetSunk ? 1 : 2, Opponent(player).Name)
			};
		} else {
			// ToDo calculate scores based on ships sunk and hits
			return new()
			{
				new(1, player.Name),
				new(1, Opponent(player).Name)
			};

		}
	}

	public bool PlaceShips(Player player, List<Ship>? shipsToPlace = null)
	{
		Board board = boards[player.Id];

		int row, col;
		Coordinate position;
		Orientation orientation;
		Ship newShip;

		if (shipsToPlace is not null) {
			foreach (Ship ship in shipsToPlace) {
				board.PlaceShip(ship);
			}
		} else {
			List<Ship> fleet = board.Fleet.ToList();
			foreach (Ship ship in fleet) {
				do {
					row = Random.Shared.Next(1, board.BoardSize + 1);
					col = Random.Shared.Next(1, board.BoardSize + 1);
					position = new(row, col);
					orientation = Random.Shared.Next(0, 2) switch
					{
						0 => Orientation.Horizontal,
						1 => Orientation.Vertical,
						_ => throw new NotImplementedException(),
					};
					newShip = new(ship.Type, position, orientation);
				} while (!board.PlaceShip(newShip));
			}
		}

		return board.IsFleetReady;
	}
	public bool PlaceShip(Player player, Ship shipToPlace) => boards[player.Id].PlaceShip(shipToPlace);
}
