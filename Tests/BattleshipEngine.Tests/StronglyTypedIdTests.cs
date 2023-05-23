namespace BattleshipEngine.Tests;

public class StronglyTypedIdTests
{
	[Fact]
	public void GameIdShouldBehaveProperly()
	{
		const string GAMEID_VALUE = "99999999-9999-9999-9999-999999999999";

		GameId gameId = new GameId();
		GameId gameId2 = new GameId();

		Assert.Equal(Guid.Empty, gameId.Value);
		
		GameId? nullableGameId = GameId.Generate();
		gameId2 = (GameId)nullableGameId;
		Assert.Equal(nullableGameId, gameId2);

		gameId = GameId.Generate();
		Assert.NotEqual(Guid.Empty, gameId.Value);

		gameId = new GameId(new Guid(GAMEID_VALUE));
		string json = JsonSerializer.Serialize(gameId);
		Assert.Equal($"\"{GAMEID_VALUE}\"", json);

		gameId2 = JsonSerializer.Deserialize<GameId>(json);
		Assert.Equal(gameId, gameId2);

		gameId2 = Task.Run(async () => await GetGameIdAsync(gameId)).Result;
		Assert.Equal(gameId, gameId2);
	}

	private static async Task<GameId> GetGameIdAsync(GameId gameId) => await Task.Run(() => gameId);
}
