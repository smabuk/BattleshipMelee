namespace BattleshipEngine.Tests;

public sealed class ShipTests
{
	[Theory]
	[InlineData(ShipType.Destroyer,  Orientation.Horizontal, "A1", "A2", 2)]
	[InlineData(ShipType.Cruiser,    Orientation.Horizontal, "A2", "A4", 3)]
	[InlineData(ShipType.Submarine,  Orientation.Horizontal, "A1", "A3", 3)]
	[InlineData(ShipType.Battleship, Orientation.Horizontal, "B6", "B9", 4)]
	[InlineData(ShipType.AircraftCarrier,    Orientation.Horizontal, "A1", "A5", 5)]
	[InlineData(ShipType.Destroyer,  Orientation.Vertical,   "A2", "B2", 2)]
	[InlineData(ShipType.Cruiser,    Orientation.Vertical,   "A1", "C1", 3)]
	[InlineData(ShipType.Submarine,  Orientation.Vertical,   "A1", "C1", 3)]
	[InlineData(ShipType.Battleship, Orientation.Vertical,   "B6", "E6", 4)]
	[InlineData(ShipType.AircraftCarrier,    Orientation.Vertical,   "A1", "E1", 5)]
	public void ShipValidWhenCreated(ShipType shipType, Orientation orientation, string startPosition, string lastPosition, int expectedSegmentCount)
	{
		Ship actual = new Ship(shipType, startPosition, orientation);
	
		Assert.Equal(orientation, actual.Orientation);
		Assert.Equal(shipType, actual.Type);
		Assert.Equal(startPosition, actual.Position);
		Assert.Equal(expectedSegmentCount, actual.Segments.Count);
		Assert.Equal(startPosition, actual.Segments.First().Key);
		Assert.Equal(lastPosition, actual.Segments.Last().Key);
	}

	[Fact]
	public void AttackABattleship()
	{
		Ship ship = new(ShipType.Battleship, "A1", Orientation.Horizontal);

		Assert.False(ship.IsSunk);
		
		AttackResult result = ship.Attack("B1");
		Assert.Equal("B1", result.AttackCoordinate);
		Assert.Equal(AttackResultType.Miss, result.HitOrMiss);
		Assert.False(ship.IsSunk);

		result = ship.Attack("A2");
		Assert.Equal(AttackResultType.Hit, result.HitOrMiss);
		Assert.Equal(ShipType.Battleship, result.ShipType);
		Assert.False(ship.IsSunk);

		result = ship.Attack("A2");
		Assert.Equal(AttackResultType.Hit, result.HitOrMiss);
		Assert.False(ship.IsSunk);

		result = ship.Attack("A1");
		Assert.Equal(AttackResultType.Hit, result.HitOrMiss);
		Assert.False(ship.IsSunk);

		result = ship.Attack("A4");
		Assert.Equal(AttackResultType.Hit, result.HitOrMiss);
		Assert.False(ship.IsSunk);

		result = ship.Attack("A3");
		Assert.Equal(AttackResultType.HitAndSunk, result.HitOrMiss);
		Assert.True(ship.IsSunk);


	}

}
