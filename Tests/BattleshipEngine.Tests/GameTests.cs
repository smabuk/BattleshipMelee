namespace BattleshipEngine.Tests;

public class GameTests
{
	readonly List<Ship> shipList = new()
		{
			new(ShipType.Cruiser,         "A1", Orientation.Horizontal),
			new(ShipType.Battleship,      "B1", Orientation.Horizontal),
			new(ShipType.Submarine,       "C1", Orientation.Horizontal),
			new(ShipType.Destroyer,       "D1", Orientation.Horizontal),
			new(ShipType.AircraftCarrier, "E5", Orientation.Vertical)
		};

	[Fact]
	public void PlayGame()
	{
		Player privatePlayer1 = new AuthPlayer("Test player");
		Player computerPlayer2 = new ComputerPlayer("Computer");
		List<Player> players = new()
		{
			privatePlayer1,
			computerPlayer2
		};

		Game game = Game.StartNewGame(players, GameType.Classic);

		Assert.Equal("Computer", computerPlayer2.Name);
		Assert.True(computerPlayer2 is ComputerPlayer);
		Assert.NotEqual(privatePlayer1, computerPlayer2);

		Assert.False(game.AreFleetsReady);

		Assert.True(game.PlaceShips(privatePlayer1, shipList));

		Assert.True(game.AreFleetsReady);

		AttackResult attackResult;
		attackResult = game.Fire(computerPlayer2, "A1");
		Assert.Equal(AttackResultType.Hit, attackResult.AttackResultType);
		attackResult = game.Fire(computerPlayer2, "A1");
		Assert.Equal(AttackResultType.AlreadyAttacked, attackResult.AttackResultType);
		attackResult = game.Fire(computerPlayer2, "J6");
		Assert.Equal(AttackResultType.Miss, attackResult.AttackResultType);

		Assert.False(game.GameOver);

		string[] attackList = {
			"A2", "A3",
			"B1", "B2", "B3", "B4",
			"C1", "C2", "C3",
			"D1", "D2",
			"E5", "F5", "G5", "H5", "I5",
		};

		foreach (string pos in attackList) {
			attackResult = game.Fire(computerPlayer2, pos);
		}

		Assert.True(game.GameOver);

		List<LeaderboardEntry> leaderboard = game.LeaderBoard().ToList();
		Assert.Equal(1, leaderboard.Single(s => s.Name == computerPlayer2.Name).Position);
		Assert.Equal(2, leaderboard.Single(s => s.Name == privatePlayer1.Name).Position);

		Assert.Equal( 148, leaderboard.Single(s => s.Name == computerPlayer2.Name).Score);
		Assert.Equal(-100, leaderboard.Single(s => s.Name == privatePlayer1.Name).Score);
	}

	[Fact]
	public void PlaceShipsIntoGame()
	{
		Player privatePlayer1 = new AuthPlayer("Test player");
		Player computerPlayer2 = new ComputerPlayer("Computer");
		List<Player> players = new()
		{
			privatePlayer1,
		};
		Game game = Game.StartNewGame(players, GameType.Classic);

		Assert.True(game.PlaceShips(privatePlayer1, shipList));

		List<Ship> ships = game.Fleet(privatePlayer1);

		Assert.Equal(5, ships.Count);

		Coordinate cruiserPosition    = ships.Single(ship => ship.Type == ShipType.Cruiser)?.Position ?? default!;
		Coordinate battleshipPosition = ships.Single(ship => ship.Type == ShipType.Battleship)?.Position ?? default!;
		Coordinate submarinePosition  = ships.Single(ship => ship.Type == ShipType.Submarine)?.Position ?? default!;
		Coordinate destroyerPosition  = ships.Single(ship => ship.Type == ShipType.Destroyer)?.Position ?? default!;
		Coordinate carrierPosition    = ships.Single(ship => ship.Type == ShipType.AircraftCarrier)?.Position ?? default!;

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
		Player privatePlayer1 = new AuthPlayer("Test player");
		List<Player> players = new()
		{
			privatePlayer1,
		};
		Game game = Game.StartNewGame(players, GameType.Classic);

		bool actual;
		if (withShips) {
			actual = game.PlaceShips(privatePlayer1, shipList, doItForMe: true);
		} else {
			actual = game.PlaceShips(privatePlayer1, doItForMe: true);
		}
		Assert.True(actual);

		List<Ship> ships = game.Fleet(privatePlayer1);

		Assert.Equal(5, ships.Count);

		Coordinate cruiserPosition    = ships.Single(ship => ship.Type == ShipType.Cruiser)?.Position ?? default!;
		Coordinate battleshipPosition = ships.Single(ship => ship.Type == ShipType.Battleship)?.Position ?? default!;
		Coordinate submarinePosition  = ships.Single(ship => ship.Type == ShipType.Submarine)?.Position ?? default!;
		Coordinate destroyerPosition  = ships.Single(ship => ship.Type == ShipType.Destroyer)?.Position ?? default!;
		Coordinate carrierPosition    = ships.Single(ship => ship.Type == ShipType.AircraftCarrier)?.Position ?? default!;

		Assert.False(
			cruiserPosition       == "A1" 
			&& battleshipPosition == "B1"
			&& submarinePosition  == "C1"
			&& destroyerPosition  == "D1"
			&& carrierPosition    == "E5" 
			);
		
	}
}
