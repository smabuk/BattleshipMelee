namespace BattleshipEngine.Tests;

public class PlayerTests
{
	readonly PrivatePlayer privatePlayer;
	private Game _game = new(GameType.Classic);

	public PlayerTests()
	{
		privatePlayer = (PrivatePlayer)_game.AddPlayer("Test player");
	}

	[Fact]
	public void UserIsWhoTheySayTheyAre()
	{
		Assert.True(privatePlayer.IsUserWhoTheySayTheyAre(privatePlayer));
	}

	[Fact]
	public void UserIsNotWhoTheySayTheyAre()
	{
		_game = new(GameType.Classic);

		PrivatePlayer player1 = new PrivatePlayer(privatePlayer.Name);
		Assert.False(privatePlayer.IsUserWhoTheySayTheyAre(player1));

		PrivatePlayer player2 = privatePlayer with { PrivateId = Guid.NewGuid() };
		Assert.False(privatePlayer.IsUserWhoTheySayTheyAre(player2));

		Player player3 = Player.PublicPlayer(privatePlayer);
		Assert.False(privatePlayer.IsUserWhoTheySayTheyAre(player3));
	}
}
