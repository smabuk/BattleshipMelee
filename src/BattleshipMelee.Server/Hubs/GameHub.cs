namespace BattleshipMelee.Server.Hubs;

internal class GameHub : Hub
{
	private readonly GameService _gameService;

	public GameHub(GameService gameService)
	{
		_gameService = gameService;
	}

	public override Task OnConnectedAsync()
	{
		return base.OnConnectedAsync();
	}

	public override Task OnDisconnectedAsync(Exception? exception)
	{
		return base.OnDisconnectedAsync(exception);
	}

	public AuthPlayer RegisterPlayer(string name)
		=> (AuthPlayer)_gameService.AddPlayer(new(Context.ConnectionId), name);

	public bool UnRegisterPlayer(AuthPlayer privatePlayer)
		=> _gameService.RemovePlayer(new(Context.ConnectionId), privatePlayer.Name);

	public async Task<GameId?> StartGameVsComputer(AuthPlayer player, string computerPlayerName = "Computer", GameType gameType = GameType.Classic)
	{
		GameId? gameId = _gameService.StartGameWithComputer(player, computerPlayerName, gameType);
		if (gameId is not null) {
			List<Player> players = _gameService.FindOpponents(player, gameId ?? default);
			await Clients.Client(Context.ConnectionId).SendAsync("Opponents", players);

			await Groups.AddToGroupAsync(Context.ConnectionId, gameId.ToString() ?? throw new ApplicationException($"{gameId} shot not be null"));
			await Clients.Client(Context.ConnectionId).SendAsync("StartGame", gameId);
		}
		return gameId;
	}

	public List<Ship> PlaceShips(AuthPlayer player, GameId gameId, List<Ship>? ships = null, bool doItForMe = false)
	{
		Clients.Client(Context.ConnectionId).SendAsync("GameStatusChange", GameStatus.PlacingShips);
		List<Ship> shipsResult = _gameService.PlaceShips(player, gameId, ships, doItForMe);
		Clients.Client(Context.ConnectionId).SendAsync("GameStatusChange", GameStatus.Attacking);
		return shipsResult;
	}

	public List<AttackResult> Fire(AuthPlayer player, GameId gameId, Coordinate attackCoordinate)
	{
		List<AttackResult> attackResults = new()
		{
			_gameService.Fire(player, gameId, attackCoordinate),
		};
		attackResults.AddRange(_gameService.ComputerPlayersFire(player, gameId, attackCoordinate));
		if (_gameService.IsGameOver(gameId)) {
			Clients.Client(Context.ConnectionId).SendAsync("GameStatusChange", GameStatus.GameOver);
		}

		Clients.Client(Context.ConnectionId).SendAsync("AttackResults", attackResults);
		return attackResults;
	}

	public ComputerPlayer FindComputerOpponent(AuthPlayer player, GameId gameId)
		=> _gameService.FindOpponents(player, gameId)
		.Where(p => p is ComputerPlayer)
		.Select(p => (ComputerPlayer)p)
		.First();

	public List<LeaderboardEntry> Leaderboard(GameId gameId)
		=> _gameService.Leaderboard(gameId);

}
