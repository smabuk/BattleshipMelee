namespace BattleshipEngine.Tests;

public class PlayerTests
{
	readonly PrivatePlayer privatePlayer;
	readonly Game game = new(GameType.Classic);

	public PlayerTests()
	{
		privatePlayer = game.AddPlayer("Test player");

	}

	[Fact]
	public void UserIsWhoTheySayTheyAre()
	{
		Assert.True(privatePlayer.IsUserWhoTheySayTheyAre(privatePlayer));
	}

	[Fact]
	public void UserIsNotWhoTheySayTheyAre()
	{
		Game game = new(GameType.Classic);

		PrivatePlayer player1 = new PrivatePlayer(privatePlayer.Name, privatePlayer.IsComputer);
		Assert.False(privatePlayer.IsUserWhoTheySayTheyAre(player1));

		PrivatePlayer player2 = privatePlayer with { PrivateId = Guid.NewGuid() };
		Assert.False(privatePlayer.IsUserWhoTheySayTheyAre(player2));

		Player player3 = Player.PublicPlayer(privatePlayer);
		Assert.False(privatePlayer.IsUserWhoTheySayTheyAre(player3));
	}
}
