using BattleshipEngine;

namespace BattleshipMelee.Server.Services;

public class GameService
{
	public ConcurrentDictionary<GameId, Game> Games = new();
	public ConcurrentDictionary<PlayerId, ConnectionId> ConnectionLookup = new();
	public ConcurrentDictionary<ConnectionId, Player> Clients = new();
	public ConcurrentDictionary<ConnectionId, PlayerStatus> PlayerStatus = new();

	public Player AddPlayer(ConnectionId connectionId, string name, bool isComputer = false)
	{
		if (Clients.ContainsKey(connectionId)) {
			return Clients[connectionId];
		}

		Player player = isComputer switch { 
			true => new ComputerPlayer("Computer"),
			false => new AuthPlayer(name),
		};

		Clients.TryAdd(connectionId, player);
		ConnectionLookup.TryAdd(player.Id, connectionId);
		return player;
	}

	internal ConnectionId GetConnectionId(PlayerId playerId) => ConnectionLookup[playerId];
	internal PlayerId GetPlayerId(ConnectionId connectionId) => Clients[connectionId].Id;

	public bool RemovePlayer(ConnectionId connectionId, string playerName)
	{
		return Clients.TryRemove(connectionId, out _);
	}

	public GameId? StartGameWithComputer(AuthPlayer player, string computerPlayerName, GameType gameType = GameType.Classic) {
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

	public List<Ship> PlaceShips(AuthPlayer player, string gameId, List<Ship>? ships, bool doItForMe = false) {
		if (Games.ContainsKey(gameId)) {
			Games[gameId].PlaceShips(player, ships, doItForMe);
			return Games[gameId].Fleet(player);
		}
		return new();
	}

	public AttackResult Fire(AuthPlayer player, string gameId, Coordinate attackCoordinate) {
		if (Games.ContainsKey(gameId)) {
			AttackResult attackResult = Games[gameId].Fire(player, attackCoordinate);
			return attackResult;
		}
		return new AttackResult(attackCoordinate, AttackResultType.InvalidPosition);
	}

	public List<AttackResult> ComputerPlayersFire(AuthPlayer player, string gameId, Coordinate attackCoordinate) {
		if (Games.ContainsKey(gameId)) {
			List<AttackResult> attackResults = Games[gameId].OtherPlayersFire().ToList();
			return attackResults;
		}
		return new List<AttackResult>();
	}

	public List<Player> FindOpponents(AuthPlayer player, string gameId) {
		if (Games.ContainsKey(gameId)) {
			return Games[gameId]._players.Where(x => x.Key != player.Id).Select(x => x.Value).ToList();
		}
		return new List<Player>();
	}

	public List<LeaderboardEntry> Leaderboard(GameId gameId) {
		List<LeaderboardEntry> leaderboard = new();
		if (Games.ContainsKey(gameId)) {
			leaderboard = Games[gameId].LeaderBoard().ToList();
		}
		return leaderboard;
	}

	public bool IsGameOver(GameId gameId) => Games.ContainsKey(gameId) ? Games[gameId].GameOver : false;

}
