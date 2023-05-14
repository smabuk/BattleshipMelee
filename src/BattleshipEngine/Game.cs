namespace BattleshipEngine;

public record Game(GameType GameType = GameType.Classic)
{
	private readonly Dictionary<PlayerId, Player> players = new();
	private readonly Dictionary<PlayerId, Board> boards = new();
	private readonly Dictionary<PlayerId, List<AttackResult>> shots = new();

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
		Player player = new(name.Trim(), isComputer);

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

		if (shots[player.Id].Any(s => s.AttackCoordinate == attackCoordinate)) {
			return new(attackCoordinate, AttackResultType.AlreadyAttacked);
		} else {
			AttackResult result = boards[playerToAttack.Id].Attack(attackCoordinate);
			shots[player.Id].Add(result);
			return result;
		}
	}

	public IEnumerable<AttackResult> FireSalvo(Player player, IEnumerable<Coordinate> attackCoordinates)
	{
		Player playerToAttack = Opponent(player);

		foreach (Coordinate attackCoordinate in attackCoordinates) {
			if (shots[player.Id].Any(s => s.AttackCoordinate == attackCoordinate)) {
				yield return new(attackCoordinate, AttackResultType.AlreadyAttacked);
			} else {
				AttackResult result = boards[playerToAttack.Id].Attack(attackCoordinate);
				shots[player.Id].Add(result);
				yield return result;
			}
		}
	}

	public IEnumerable<PlayerFinishingPosition> LeaderBoard(Player focusedPlayer)
	{
		List<PlayerFinishingPosition> playerFinishingPositions = new();

		foreach (Player player in players.Values) {
			int score = 0;
			foreach (AttackResult shot in shots[player.Id]) {
				score += shot.HitOrMiss switch
				{
					AttackResultType.Hit => 2,
					AttackResultType.Miss => 0,
					AttackResultType.HitAndSunk => 5,
					AttackResultType.AlreadyAttacked => -1,
					AttackResultType.InvalidPosition => -1,
					_ => 0,
				};
			}
			score += boards[player.Id].IsFleetSunk ? -100 : 0;
			score += boards[Opponent(player).Id].IsFleetSunk ? 100 : 0;
			PlayerFinishingPosition playerFinishingPosition = new(player.Name, Score: score);
			playerFinishingPositions.Add(playerFinishingPosition);
		}

		int position = 0;
		int previousScore = int.MaxValue;
		foreach (PlayerFinishingPosition playerFinishingPosition in playerFinishingPositions.OrderByDescending(p => p.Score)) {
			if (playerFinishingPosition.Score < previousScore) {
				previousScore = playerFinishingPosition.Score;
				position++;
			}
			yield return playerFinishingPosition with { Position = position };
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
