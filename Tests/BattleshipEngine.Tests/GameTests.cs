namespace BattleshipEngine.Tests;

public class GameTests
{
	[Fact]
	public void PlayGame()
	{
		Game game = new(GameType.Classic);

		Player player1 = game.AddPlayer("Test player");
		Player player2 = game.AddPlayer("Computer", isComputer: true);
		Assert.Equal("Computer", player2.Name);
		Assert.True(player2.IsComputer);
		Assert.NotEqual(player1, player2);

		Assert.False(game.AreFleetsReady);

		List<Ship> shipList = new()
		{
			new(ShipType.Cruiser, "A1", Orientation.Horizontal),
			new(ShipType.Battleship, "B1", Orientation.Horizontal),
			new(ShipType.Submarine, "C1", Orientation.Horizontal),
			new(ShipType.Destroyer, "D1", Orientation.Horizontal),
			new(ShipType.Carrier, "E5", Orientation.Vertical)
		};
		Assert.True(game.PlaceShips(player1, shipList));

		Assert.True(game.AreFleetsReady);

		AttackResult attackResult;
		attackResult = game.Fire(player2, "A1");
		Assert.Equal(AttackResultType.Hit, attackResult.HitOrMiss);
		attackResult = game.Fire(player2, "A1");
		Assert.Equal(AttackResultType.AlreadyAttacked, attackResult.HitOrMiss);
		attackResult = game.Fire(player2, "J6");
		Assert.Equal(AttackResultType.Miss, attackResult.HitOrMiss);

		Assert.False(game.GameOver);

		string[] attackList = {
			"A2", "A3",
			"B1", "B2", "B3", "B4",
			"C1", "C2", "C3",
			"D1", "D2",
			"E5", "F5", "G5", "H5", "I5",
		};

		foreach (string pos in attackList) {
			attackResult = game.Fire(player2, pos);
		}

		Assert.True(game.GameOver);

		List<PlayerFinishingPosition> playerFinishingPositions = game.LeaderBoard(player1).ToList();
		Assert.Equal(1, playerFinishingPositions.Single(s => s.Name == player2.Name).Position);
		Assert.Equal(2, playerFinishingPositions.Single(s => s.Name == player1.Name).Position);
	}
}
