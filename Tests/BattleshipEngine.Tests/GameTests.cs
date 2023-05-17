namespace BattleshipEngine.Tests;

public class GameTests
{
	readonly List<Ship> shipList = new()
		{
			new(ShipType.Cruiser,    "A1", Orientation.Horizontal),
			new(ShipType.Battleship, "B1", Orientation.Horizontal),
			new(ShipType.Submarine,  "C1", Orientation.Horizontal),
			new(ShipType.Destroyer,  "D1", Orientation.Horizontal),
			new(ShipType.AircraftCarrier,    "E5", Orientation.Vertical)
		};

	[Fact]
	public void PlayGame()
	{
		Game game = new(GameType.Classic);

		PrivatePlayer privatePlayer1 = game.AddPlayer("Test player");
		PrivatePlayer privatePlayer2 = game.AddPlayer("Computer", isComputer: true);
		Assert.Equal("Computer", privatePlayer2.Name);
		Assert.True(privatePlayer2.IsComputer);
		Assert.NotEqual(privatePlayer1, privatePlayer2);

		Assert.False(game.AreFleetsReady);

		Assert.True(game.PlaceShips(privatePlayer1, shipList));

		Assert.True(game.AreFleetsReady);

		AttackResult attackResult;
		attackResult = game.Fire(privatePlayer2, "A1");
		Assert.Equal(AttackResultType.Hit, attackResult.HitOrMiss);
		attackResult = game.Fire(privatePlayer2, "A1");
		Assert.Equal(AttackResultType.AlreadyAttacked, attackResult.HitOrMiss);
		attackResult = game.Fire(privatePlayer2, "J6");
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
			attackResult = game.Fire(privatePlayer2, pos);
		}

		Assert.True(game.GameOver);

		List<PlayerWithScore> leaderboard = game.LeaderBoard().ToList();
		Assert.Equal(1, leaderboard.Single(s => s.Player.Id == privatePlayer2.Id).Position);
		Assert.Equal(2, leaderboard.Single(s => s.Player.Id == privatePlayer1.Id).Position);
	}

	[Fact]
	public void PlaceShipsIntoGame()
	{
		Game game = new(GameType.Classic);

		PrivatePlayer privatePlayer1 = game.AddPlayer("Test player");
		Assert.True(game.PlaceShips(privatePlayer1, shipList));

		List<Ship> ships = game.Fleet(privatePlayer1);

		Assert.Equal(5, ships.Count);

		Coordinate cruiserPosition    = ships.Single(ship => ship.Type == ShipType.Cruiser).Position;
		Coordinate battleshipPosition = ships.Single(ship => ship.Type == ShipType.Battleship).Position;
		Coordinate submarinePosition  = ships.Single(ship => ship.Type == ShipType.Submarine).Position;
		Coordinate destroyerPosition  = ships.Single(ship => ship.Type == ShipType.Destroyer).Position;
		Coordinate carrierPosition    = ships.Single(ship => ship.Type == ShipType.AircraftCarrier).Position;

		Assert.True(
			cruiserPosition       == "A1" 
			&& battleshipPosition == "B1"
			&& submarinePosition  == "C1"
			&& destroyerPosition  == "D1"
			&& carrierPosition    == "E5" 
			);
		
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void PlaceShipsRandomlyIntoGame(bool withShips)
	{
		Game game = new(GameType.Classic);

		PrivatePlayer privatePlayer1 = game.AddPlayer("Test player");
		bool actual;
		if (withShips) {
			actual = game.PlaceShips(privatePlayer1, shipList, doItForMe: true);
		} else {
			actual = game.PlaceShips(privatePlayer1, doItForMe: true);
		}
		Assert.True(actual);

		List<Ship> ships = game.Fleet(privatePlayer1);

		Assert.Equal(5, ships.Count);

		Coordinate cruiserPosition    = ships.Single(ship => ship.Type == ShipType.Cruiser).Position;
		Coordinate battleshipPosition = ships.Single(ship => ship.Type == ShipType.Battleship).Position;
		Coordinate submarinePosition  = ships.Single(ship => ship.Type == ShipType.Submarine).Position;
		Coordinate destroyerPosition  = ships.Single(ship => ship.Type == ShipType.Destroyer).Position;
		Coordinate carrierPosition    = ships.Single(ship => ship.Type == ShipType.AircraftCarrier).Position;

		Assert.False(
			cruiserPosition       == "A1" 
			&& battleshipPosition == "B1"
			&& submarinePosition  == "C1"
			&& destroyerPosition  == "D1"
			&& carrierPosition    == "E5" 
			);
		
	}
}
