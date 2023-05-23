namespace BattleshipEngine.Tests;

public class PlayerTests
{
	readonly AuthPlayer privatePlayer;
	private Game _game;

	public PlayerTests()
	{
		privatePlayer = new AuthPlayer("Test player");
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
		AuthPlayer player1 = new AuthPlayer(privatePlayer.Name);
		Assert.False(privatePlayer.IsUserWhoTheySayTheyAre(player1));

		AuthPlayer player2 = privatePlayer with { PrivateId = PlayerId.Generate() };
		Assert.False(privatePlayer.IsUserWhoTheySayTheyAre(player2));

		Player player3 = Player.PublicPlayer(privatePlayer);
		Assert.False(privatePlayer.IsUserWhoTheySayTheyAre(player3));
	}
}
