namespace BattleshipMelee.Server.Services;

public class GameService
{
	public ConcurrentDictionary<GameId, Game> Games = new();
	public ConcurrentDictionary<ConnectionId, Player> Clients = new();
	public ConcurrentDictionary<ConnectionId, PlayerStatus> PlayerStatus = new();

	public Player AddPlayer(ConnectionId connectionId, string name, bool isComputer = false)
	{
		if (Clients.ContainsKey(connectionId)) {
			return Clients[connectionId];
		}

		Player player = isComputer switch { 
			true => new ComputerPlayer("Computer"),
			false => new PrivatePlayer(name),
		};

		Clients.TryAdd(connectionId, player);
		return player;
	}

	public bool RemovePlayer(ConnectionId connectionId, string playerName)
	{
		return Clients.TryRemove(connectionId, out _);
	}
}
