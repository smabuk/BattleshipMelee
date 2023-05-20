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

	public PrivatePlayer RegisterPlayer(string name)
	{
		return (PrivatePlayer)_gameService.AddPlayer(Context.ConnectionId, name);
	}

	public void UnRegisterPlayer(PrivatePlayer privatePlayer)
	{
		_gameService.RemovePlayer(Context.ConnectionId, privatePlayer.Name);
	}

	public string? StartGameVsComputer(PrivatePlayer player, string computerPlayerName = "Computer", GameType gameType = GameType.Classic)
	{
		return _gameService.StartGameWithComputer(player, computerPlayerName, gameType);
	}

	public List<Ship> PlaceShips(PrivatePlayer player, string gameId, List<Ship>? ships = null, bool doItForMe = false)
	{
		return _gameService.PlaceShips(player, gameId, ships, doItForMe);
	}



}
