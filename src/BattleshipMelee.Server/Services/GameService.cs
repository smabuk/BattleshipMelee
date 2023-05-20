namespace BattleshipMelee.Server.Services;

public class GameService
{
	private ConcurrentDictionary<GameId, Game> _games = new();
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

	public GameId? StartGameWithComputer(PrivatePlayer player, string computerPlayerName, GameType gameType = GameType.Classic) {
		List<Player> players = new() {
			player,
			new ComputerPlayer(computerPlayerName),
		};
		Game game = Game.StartNewGame(players, gameType);
		if (_games.TryAdd(game.GameId, game)) { 
			return game.GameId;
		};
		return null;
	}
}
