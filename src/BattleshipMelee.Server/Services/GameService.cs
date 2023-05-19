namespace BattleshipMelee.Server.Services;

public class GameService
{
	public Dictionary<string, PrivatePlayer> Clients = new();

	public PrivatePlayer AddPlayer(string connectionId, string name, bool isComputer = false)
	{
		if (Clients.ContainsKey(connectionId)) {
			return Clients[connectionId];
		}

		PrivatePlayer player = isComputer switch { 
			true => new PrivatePlayer("Computer", true),
			false => new PrivatePlayer(name),
		};

		Clients.Add(connectionId, player);
		return player;
	}

	public bool RemovePlayer(string connectionId, string name, bool isComputer = false)
	{
		return Clients.Remove(connectionId);
	}
}
