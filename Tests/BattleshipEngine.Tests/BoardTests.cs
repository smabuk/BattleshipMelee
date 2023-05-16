namespace BattleshipEngine.Tests;

public class BoardTests
{
	[Fact]
	public void CreateAClassicBoard()
	{
		Board board = new Board(10) { Fleet = Game.GameShips(GameType.Classic) };

		List<Ship> ships = board.Fleet.Where(s => s.IsPositioned == false).ToList();
		Assert.Equal(5, ships.Count);
	}

	[Fact]
	public void PlaceShipsOnBoard()
	{
		Board board = new Board(10) { Fleet = Game.GameShips(GameType.Classic) };

		bool success = board.PlaceShip(new(ShipType.Cruiser, "A1", Orientation.Horizontal));
		Assert.True(success);

		// Ship type has already been placed 
		success = board.PlaceShip(new(ShipType.Cruiser, "B1", Orientation.Horizontal));
		Assert.False(success, "The Cruiser should have already been placed.");

		// Overlaps with another ship 
		success = board.PlaceShip(new(ShipType.Battleship, "A1", Orientation.Horizontal));
		Assert.False(success, "The ship should overlap position A1.");

		// Part of the ship is off the board 
		success = board.PlaceShip(new(ShipType.Cruiser, "J9", Orientation.Horizontal));
		Assert.False(success, "The back end of ship should be outside the board at J11.");

		success = board.PlaceShip(new(ShipType.Battleship, "B1", Orientation.Horizontal));
		Assert.True(success);

		success = board.PlaceShip(new(ShipType.Submarine, "C1", Orientation.Horizontal));
		Assert.True(success);

		success = board.PlaceShip(new(ShipType.Destroyer, "D1", Orientation.Horizontal));
		Assert.True(success);

		success = board.PlaceShip(new(ShipType.Carrier, "E5", Orientation.Vertical));
		Assert.True(success);

		Assert.Equal(5, board.Fleet.Count(ship => ship.IsPositioned));
		Assert.Equal(0, board.Fleet.Count(ship => ship.IsPositioned == false));
		Assert.Equal(0, board.Fleet.Count(ship => ship.IsSunk));

		Assert.False(board.IsFleetSunk);

	}

	[Fact]
	public void AttackBoard()
	{
		Board board = new Board(10) { Fleet = Game.GameShips(GameType.Classic) };

		_ = board.PlaceShip(new(ShipType.Cruiser, "A1", Orientation.Horizontal));
		_ = board.PlaceShip(new(ShipType.Battleship, "B1", Orientation.Horizontal));
		_ = board.PlaceShip(new(ShipType.Submarine, "C1", Orientation.Horizontal));
		_ = board.PlaceShip(new(ShipType.Destroyer, "D1", Orientation.Horizontal));
		_ = board.PlaceShip(new(ShipType.Carrier, "E5", Orientation.Vertical));
		Assert.False(board.IsFleetSunk);

		AttackResult actual;

		actual = board.Attack("A1");
		Assert.Equal(new("A1", AttackResultType.Hit, ShipType.Cruiser), actual);
		actual = board.Attack("A2");
		Assert.Equal(new("A2", AttackResultType.Hit, ShipType.Cruiser), actual);
		actual = board.Attack("A3");
		Assert.Equal(new("A3", AttackResultType.HitAndSunk, ShipType.Cruiser), actual);
		actual = board.Attack("A2");
		Assert.Equal(new("A2", AttackResultType.HitAndSunk, ShipType.Cruiser), actual);
		actual = board.Attack("A4");
		Assert.Equal(new("A4", AttackResultType.Miss, null), actual);
		Assert.False(board.IsFleetSunk);

		actual = board.Attack("B1");
		Assert.Equal(new("B1", AttackResultType.Hit, ShipType.Battleship), actual);
		actual = board.Attack("B2");
		Assert.Equal(new("B2", AttackResultType.Hit, ShipType.Battleship), actual);
		actual = board.Attack("B3");
		Assert.Equal(new("B3", AttackResultType.Hit, ShipType.Battleship), actual);
		actual = board.Attack("B4");
		Assert.Equal(new("B4", AttackResultType.HitAndSunk, ShipType.Battleship), actual);
		Assert.False(board.IsFleetSunk);

		actual = board.Attack("C1");
		Assert.Equal(new("C1", AttackResultType.Hit, ShipType.Submarine), actual);
		actual = board.Attack("C2");
		Assert.Equal(new("C2", AttackResultType.Hit, ShipType.Submarine), actual);
		actual = board.Attack("C3");
		Assert.Equal(new("C3", AttackResultType.HitAndSunk, ShipType.Submarine), actual);
		Assert.False(board.IsFleetSunk);

		actual = board.Attack("D2");
		Assert.Equal(new("D2", AttackResultType.Hit, ShipType.Destroyer), actual);
		actual = board.Attack("D1");
		Assert.Equal(new("D1", AttackResultType.HitAndSunk, ShipType.Destroyer), actual);
		Assert.False(board.IsFleetSunk);

		actual = board.Attack("E5");
		Assert.Equal(new("E5", AttackResultType.Hit, ShipType.Carrier), actual);
		actual = board.Attack("F5");
		Assert.Equal(new("F5", AttackResultType.Hit, ShipType.Carrier), actual);
		actual = board.Attack("G5");
		Assert.Equal(new("G5", AttackResultType.Hit, ShipType.Carrier), actual);
		actual = board.Attack("H5");
		Assert.Equal(new("H5", AttackResultType.Hit, ShipType.Carrier), actual);
		actual = board.Attack("I5");
		Assert.Equal(new("I5", AttackResultType.HitAndSunk, ShipType.Carrier), actual);
		Assert.True(board.IsFleetSunk);

	}
}
