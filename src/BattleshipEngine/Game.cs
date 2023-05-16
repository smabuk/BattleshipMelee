namespace BattleshipEngine;

public record Game(GameType GameType = GameType.Classic)
{
	private readonly Dictionary<PlayerId, PrivatePlayer> players = new();
	private readonly Dictionary<PlayerId, Board> boards = new();
	private readonly Dictionary<PlayerId, List<AttackResult>> shots = new();

	public void Init()
	{
		foreach (Player player in players.Values) {
			shots[player.Id] = new();
		}
	}

	public bool AreFleetsReady => boards.Values.All(board => board.IsFleetReady);
	public int BoardSize => GetBoardSize(GameType);
	public bool GameOver => boards.Values.Any(board => board.IsFleetSunk);
	public List<Ship> Fleet(Player player) => boards[player.Id].Fleet.ToList();
	public string OpponentName(Player player) => Opponent(player).Name;
	private Player Opponent(Player player) => Player.PublicPlayer(players.Values.Single(p => p.Id != player.Id));

	public PrivatePlayer AddPlayer(string name, bool isComputer = false)
	{
		PrivatePlayer privatePlayer = new(name.Trim(), isComputer);

		players.Add(privatePlayer.Id, privatePlayer);
		boards.Add(privatePlayer.Id, new(BoardSize) { Fleet = GameShips(GameType) });

		shots[privatePlayer.Id] = new();

		if (isComputer) {
			PlaceShips(privatePlayer);
		}

		return privatePlayer;
	}

	public AttackResult Fire(PrivatePlayer privatePlayer, Coordinate attackCoordinate)
	{
		Player playerToAttack = Opponent(privatePlayer);

		if (shots[privatePlayer.Id].Any(s => s.AttackCoordinate == attackCoordinate)) {
			return new(attackCoordinate, AttackResultType.AlreadyAttacked) { TargetedPlayer = playerToAttack };
		} else {
			AttackResult result = boards[playerToAttack.Id].Attack(attackCoordinate) with { TargetedPlayer = playerToAttack };
			shots[privatePlayer.Id].Add(result);
			return result;
		}
	}

	public IEnumerable<AttackResult> FireSalvo(PrivatePlayer privatePlayer, IEnumerable<Coordinate> attackCoordinates)
	{
		Player playerToAttack = Opponent(privatePlayer);

		foreach (Coordinate attackCoordinate in attackCoordinates) {
			if (shots[privatePlayer.Id].Any(s => s.AttackCoordinate == attackCoordinate)) {
				yield return new(attackCoordinate, AttackResultType.AlreadyAttacked) { TargetedPlayer = playerToAttack };
			} else {
				AttackResult result = boards[playerToAttack.Id].Attack(attackCoordinate) with { TargetedPlayer = playerToAttack };
				shots[privatePlayer.Id].Add(result);
				yield return result;
			}
		}
	}

	public IEnumerable<PlayerWithScore> LeaderBoard()
	{
		List<PlayerWithScore> leaderboard = new();

		foreach (Player privatePlayer in players.Values) {
			Player player = Player.PublicPlayer(privatePlayer);
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
			PlayerWithScore leaderboardPosition = new(player, Score: score);
			leaderboard.Add(leaderboardPosition);
		}

		int position = 0;
		int previousScore = int.MaxValue;
		foreach (PlayerWithScore leaderboardPosition in leaderboard.OrderByDescending(p => p.Score)) {
			if (leaderboardPosition.Score < previousScore) {
				previousScore = leaderboardPosition.Score;
				position++;
			}
			yield return leaderboardPosition with { Position = position };
		}
	}

	public bool PlaceShips(PrivatePlayer privatePlayer, List<Ship>? shipsToPlace = null)
	{
		bool validUser = IsUserWhoTheySayTheyAre(privatePlayer);
		if (validUser is false) {
			return false;
		}

		Board board = boards[privatePlayer.Id];

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

	public bool PlaceShip(PrivatePlayer privatePlayer, Ship shipToPlace)
	{
		bool validUser = IsUserWhoTheySayTheyAre(privatePlayer);
		if (validUser is false) {
			return false;
		}

		return boards[privatePlayer.Id].PlaceShip(shipToPlace);
	}

	private bool IsUserWhoTheySayTheyAre(PrivatePlayer privatePlayer)
	{
		PrivatePlayer storedPlayer = players[privatePlayer.Id];
		if (storedPlayer.IsUserWhoTheySayTheyAre(privatePlayer)) {
			return true;
		}
		//throw new AuthenticationException();
		return false;
	}

	public static int GetBoardSize(GameType gameType) => gameType switch
	{
		GameType.Classic => 10,
		GameType.Melee => throw new NotImplementedException(),
		GameType.BigBangTheory => throw new NotImplementedException(),
		_ => throw new NotImplementedException(),
	};

	public static List<Ship> GameShips(GameType gameType)
	{
		List<Ship> fleet = gameType switch
		{
			GameType.Classic => new ()
			{
				new (ShipType.Destroyer),
				new (ShipType.Submarine),
				new (ShipType.Cruiser),
				new (ShipType.Battleship),
				new (ShipType.Carrier),
			},
			GameType.BigBangTheory => new ()
			{
				new (ShipType.Destroyer),
				new (ShipType.Submarine),
				new (ShipType.Cruiser),
				new (ShipType.RomulanBattleBagel),
				new (ShipType.Carrier),
			},
			GameType.Melee => throw new NotImplementedException(),
			_ => throw new NotImplementedException(),
		};

		return fleet;
	}

}
