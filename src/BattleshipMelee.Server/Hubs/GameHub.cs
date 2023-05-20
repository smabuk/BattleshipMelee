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

	public ComputerPlayer FindComputerOpponent()
	{
		ComputerPlayer opponent = (ComputerPlayer)_gameService.AddPlayer("Computer", "Computer", isComputer: true);

		return opponent;
	}

	public IEnumerable<Ship> PlaceMyShips(GameId gameId, Player player, IEnumerable<Ship>? ships = null, bool doItForMe = false)
	{
		//ComputerPlayer opponent = (ComputerPlayer)_gameService.AddPlayer("Computer", "Computer", isComputer: true);

		return ships ?? new List<Ship>();
	}



}
