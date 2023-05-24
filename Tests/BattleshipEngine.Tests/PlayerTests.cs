namespace BattleshipEngine.Tests;

public class PlayerTests
{
	readonly Player                 player = new("Just a player");
	readonly AuthPlayer         authPlayer = new("Auth player");
	readonly ComputerPlayer computerPlayer = new("Computer player");

	[Fact]
	public void SerializeDeserializeAuthPlayer()
	{
		string expected = $$"""{"PrivateId":"{{authPlayer.PrivateId}}","Name":"{{authPlayer.Name}}","Id":"{{authPlayer.Id}}"}""";
		string actualJson = JsonSerializer.Serialize(authPlayer);
		Assert.Equal(expected, actualJson);

		AuthPlayer? actual = JsonSerializer.Deserialize<AuthPlayer>(actualJson);
		Assert.Equal(authPlayer, actual);
	}

	[Fact]
	public void SerializeDeserializeComputerPlayer()
	{
		string expected = $$"""{"Name":"{{computerPlayer.Name}}","Id":"{{computerPlayer.Id}}"}""";
		string actualJson = JsonSerializer.Serialize(computerPlayer);
		Assert.Equal(expected, actualJson);

		ComputerPlayer? actual = JsonSerializer.Deserialize<ComputerPlayer>(actualJson);
		Assert.Equal(computerPlayer, actual);
	}

	[Fact]
	public void SerializeDeserializePlayer()
	{
		string expected = $$"""{"Name":"{{player.Name}}","Id":"{{player.Id}}"}""";
		string actualJson = JsonSerializer.Serialize(player);
		Assert.Equal(expected, actualJson);

		Player? actual = JsonSerializer.Deserialize<Player>(actualJson);
		Assert.Equal(player, actual);
	}

	[Fact(Skip = "Cannot cast from ComputerPlayer or AuthPlayer to Player.")]
	public void SerializeDeserializePlayers()
	{
		List<Player> players = new()
		{
			new Player("Just a player"),
			new ComputerPlayer("Computer player"),
			new AuthPlayer("Auth player"),
		};

		string actualJson = JsonSerializer.Serialize(players);
		Assert.Equal(208, actualJson.Length);

		List<Player>? actual = JsonSerializer.Deserialize<List<Player>>(actualJson);
		Assert.NotNull(actual);
		Assert.Equal(3, actual?.Count);

		Assert.Contains(players[0], actual!);
		Assert.Contains(players[1], actual!);
		Assert.Contains(players[2], actual!);
	}

	[Fact]
	public void UserIsWhoTheySayTheyAre()
	{
		Assert.True(authPlayer.IsUserWhoTheySayTheyAre(authPlayer));
	}

	[Fact]
	public void UserIsNotWhoTheySayTheyAre()
	{
		AuthPlayer player1 = new AuthPlayer(authPlayer.Name);
		Assert.False(authPlayer.IsUserWhoTheySayTheyAre(player1));

		AuthPlayer player2 = authPlayer with { PrivateId = PlayerId.Generate() };
		Assert.False(authPlayer.IsUserWhoTheySayTheyAre(player2));

		Player player3 = Player.PublicPlayer(authPlayer);
		Assert.False(authPlayer.IsUserWhoTheySayTheyAre(player3));
	}
}
