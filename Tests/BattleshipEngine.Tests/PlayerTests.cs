namespace BattleshipEngine.Tests;

public class PlayerTests
{
	readonly PrivatePlayer privatePlayer;
	private Game _game;

	public PlayerTests()
	{
		privatePlayer = new PrivatePlayer("Test player");
		List<Player> players = new()
		{
			privatePlayer
		};
		_game = Game.StartNewGame(players, GameType.Classic);
	}

	[Fact]
	public void UserIsWhoTheySayTheyAre()
	{
		Assert.True(privatePlayer.IsUserWhoTheySayTheyAre(privatePlayer));
	}

	[Fact]
	public void UserIsNotWhoTheySayTheyAre()
	{
		PrivatePlayer player1 = new PrivatePlayer(privatePlayer.Name);
		Assert.False(privatePlayer.IsUserWhoTheySayTheyAre(player1));

		PrivatePlayer player2 = privatePlayer with { PrivateId = Guid.NewGuid().ToString() };
		Assert.False(privatePlayer.IsUserWhoTheySayTheyAre(player2));

		Player player3 = Player.PublicPlayer(privatePlayer);
		Assert.False(privatePlayer.IsUserWhoTheySayTheyAre(player3));
	}
}
