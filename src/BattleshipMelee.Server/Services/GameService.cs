namespace BattleshipMelee.Server.Services;

public class GameService
{
	public ConcurrentDictionary<string, Game> Games = new();
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

	public string? StartGameWithComputer(PrivatePlayer player, string computerPlayerName, GameType gameType = GameType.Classic) {
		List<Player> players = new() {
			player,
			new ComputerPlayer(computerPlayerName),
		};
		Game game = Game.StartNewGame(players, gameType);
		if (Games.TryAdd(game.GameId, game)) {
			return game.GameId;
		};
		return null;
	}

	public List<Ship> PlaceShips(PrivatePlayer player, string gameId, List<Ship>? ships, bool doItForMe = false) {
		if (Games.ContainsKey(gameId)) {
			Games[gameId].PlaceShips(player, ships, doItForMe);
			return Games[gameId].Fleet(player);
		}
		return new();
	}


}
