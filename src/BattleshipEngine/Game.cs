namespace BattleshipEngine;

public record Game(GameType GameType = GameType.Classic)
{
	public readonly Dictionary<PlayerId, Player> _players = new();
	private readonly Dictionary<PlayerId, Board> _boards = new();
	private readonly Dictionary<PlayerId, List<AttackResult>> _shots = new();

	public GameId GameId { get; private set; }
	public bool AreFleetsReady => _boards.Values.All(board => board.IsFleetReady);
	public int BoardSize => GetBoardSize(GameType);
	public bool GameOver => _boards.Values.Any(board => board.IsFleetSunk);
	public List<Ship> Fleet(Player player) => _boards[player.Id].Fleet.ToList();
	public string OpponentName(Player player) => Opponent(player).Name;
	private Player Opponent(Player player) => Player.PublicPlayer(_players.Values.Single(p => p.Id != player.Id));

	public static Game StartNewGame(List<Player> players, GameType gameType = GameType.Classic)
	{
		Game game = new Game(gameType) {
			GameId = GameId.Generate(),
		};
		foreach (Player player in players) {
			game.AddPlayer(player);
		}
		return game;
	}

	internal Player AddPlayer(Player player)
	{
		_players.Add(player.Id, player);
		_boards.Add(player.Id, new(BoardSize) { Fleet = GameShips(GameType) });

		_shots[player.Id] = new();

		if (player is ComputerPlayer) {
			PlaceShips(player, doItForMe: true);
		}

		return player;
	}

	internal Player AddPlayer(string name, bool isComputer = false)
	{
		Player privatePlayer = isComputer switch
		{
			false => new AuthPlayer(name.Trim()),
			true => new ComputerPlayer(name.Trim()),
		};

		_players.Add(privatePlayer.Id, privatePlayer);
		_boards.Add(privatePlayer.Id, new(BoardSize) { Fleet = GameShips(GameType) });

		_shots[privatePlayer.Id] = new();

		if (isComputer) {
			PlaceShips(privatePlayer, doItForMe: true);
		}

		return privatePlayer;
	}

	public AttackResult Fire(Player player, Coordinate attackCoordinate)
	{
		Player playerToAttack = Opponent(player);

		if (_shots[player.Id].Any(s => s.AttackCoordinate == attackCoordinate)) {
			return new(attackCoordinate, AttackResultType.AlreadyAttacked) { TargetedPlayerId = playerToAttack.Id };
		} else {
			AttackResult result = _boards[playerToAttack.Id].Attack(attackCoordinate) with { TargetedPlayerId = playerToAttack.Id };
			_shots[player.Id].Add(result);
			return result;
		}
	}

	public IEnumerable<AttackResult> FireSalvo(Player privatePlayer, IEnumerable<Coordinate> attackCoordinates)
	{
		Player playerToAttack = Opponent(privatePlayer);

		foreach (Coordinate attackCoordinate in attackCoordinates) {
			if (_shots[privatePlayer.Id].Any(s => s.AttackCoordinate == attackCoordinate)) {
				yield return new(attackCoordinate, AttackResultType.AlreadyAttacked) { TargetedPlayerId = playerToAttack.Id };
			} else {
				AttackResult result = _boards[playerToAttack.Id].Attack(attackCoordinate) with { TargetedPlayerId = playerToAttack.Id };
				_shots[privatePlayer.Id].Add(result);
				yield return result;
			}
		}
	}

	public IEnumerable<AttackResult> OtherPlayersFire()
	{
		foreach (ComputerPlayer computerPlayer in _players.Values.Where(p => p is ComputerPlayer)) {
			Player playerToAttack = Opponent(computerPlayer);

			HashSet<Coordinate> alreadyAttacked = _shots[computerPlayer.Id].Select(s => s.AttackCoordinate).ToHashSet();
			Coordinate attackCoordinate = new(Random.Shared.Next(1, 11), Random.Shared.Next(1, 11));
			while (alreadyAttacked.Contains(attackCoordinate)) {
				attackCoordinate = new(Random.Shared.Next(1, 11), Random.Shared.Next(1, 11));
			}

			AttackResult result = _boards[playerToAttack.Id].Attack(attackCoordinate) with { TargetedPlayerId = playerToAttack.Id };
			_shots[computerPlayer.Id].Add(result);
			yield return result;
		}

	}

	public IEnumerable<LeaderboardEntry> LeaderBoard()
	{
		List<RankedPlayer> leaderboard = new();

		foreach (Player privatePlayer in _players.Values) {
			Player player = Player.PublicPlayer(privatePlayer);
			int score = 0;
			foreach (AttackResult shot in _shots[player.Id]) {
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
			score += _boards[player.Id].IsFleetSunk ? -100 : 0;
			score += _boards[Opponent(player).Id].IsFleetSunk ? 100 : 0;
			RankedPlayer leaderboardPosition = new(player, Score: score);
			leaderboard.Add(leaderboardPosition);
		}

		int position = 0;
		int previousScore = int.MaxValue;
		foreach (RankedPlayer leaderboardPosition in leaderboard.OrderByDescending(p => p.Score)) {
			if (leaderboardPosition.Score < previousScore) {
				previousScore = leaderboardPosition.Score;
				position++;
			}
			yield return new LeaderboardEntry(
				leaderboardPosition.Player.Name,
				position,
				leaderboardPosition.Score,
				leaderboardPosition.Player is ComputerPlayer);
		}
	}

	public bool PlaceShips(Player privatePlayer, List<Ship>? shipsToPlace = null, bool doItForMe = false)
	{
		//bool validUser = IsUserWhoTheySayTheyAre(privatePlayer);
		//if (validUser is false) {
		//	return false;
		//}

		Board board = _boards[privatePlayer.Id];

		int row, col;
		Coordinate position;
		Orientation orientation;
		Ship newShip;

		if (shipsToPlace is not null && doItForMe is false) {
			foreach (Ship ship in shipsToPlace) {
				board.PlaceShip(ship);
			}
		} else {
			List<Ship> fleet; 
			if (shipsToPlace is null) {
				fleet = board.Fleet.ToList();
			} else {
				fleet = shipsToPlace.ToList();
			}
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

	public bool PlaceShip(Player privatePlayer, Ship shipToPlace)
	{
		bool validUser = IsUserWhoTheySayTheyAre(privatePlayer);
		if (validUser is false) {
			return false;
		}

		return _boards[privatePlayer.Id].PlaceShip(shipToPlace);
	}

	private bool IsUserWhoTheySayTheyAre(Player privatePlayer)
	{
		AuthPlayer storedPlayer = (AuthPlayer)_players[privatePlayer.Id];
		if (storedPlayer.IsUserWhoTheySayTheyAre(privatePlayer)) {
			return true;
		}
		//throw new AuthenticationException();
		return false;
	}

	public static int GetBoardSize(GameType gameType) => gameType switch
	{
		GameType.Classic => 10,
		GameType.Melee => throw new NotImplementedException("Melee is not implemented yet"),
		GameType.BigBangTheory => 10,
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
				new (ShipType.AircraftCarrier),
			},
			GameType.BigBangTheory => new ()
			{
				new (ShipType.Destroyer),
				new (ShipType.Submarine),
				new (ShipType.Cruiser),
				new (ShipType.RomulanBattleBagel),
				new (ShipType.AircraftCarrier),
			},
			GameType.Melee => throw new NotImplementedException(),
			_ => throw new NotImplementedException(),
		};

		return fleet;
	}

}
