namespace BattleshipEngine.Tests;

public sealed class ShipTests
{
	[Theory]
	[InlineData(ShipType.Destroyer,        Orientation.Horizontal, "A1", "A2", 2)]
	[InlineData(ShipType.Cruiser,          Orientation.Horizontal, "A2", "A4", 3)]
	[InlineData(ShipType.Submarine,        Orientation.Horizontal, "A1", "A3", 3)]
	[InlineData(ShipType.Battleship,       Orientation.Horizontal, "B6", "B9", 4)]
	[InlineData(ShipType.AircraftCarrier,  Orientation.Horizontal, "A1", "A5", 5)]
	[InlineData(ShipType.Destroyer,        Orientation.Vertical,   "A2", "B2", 2)]
	[InlineData(ShipType.Cruiser,          Orientation.Vertical,   "A1", "C1", 3)]
	[InlineData(ShipType.Submarine,        Orientation.Vertical,   "A1", "C1", 3)]
	[InlineData(ShipType.Battleship,       Orientation.Vertical,   "B6", "E6", 4)]
	[InlineData(ShipType.AircraftCarrier,  Orientation.Vertical,   "A1", "E1", 5)]
	public void ShipValidWhenCreated(ShipType shipType, Orientation orientation, string startPosition, string lastPosition, int expectedSegmentCount)
	{
		Ship actual = new Ship(shipType, startPosition, orientation);
	
		Assert.Equal(orientation,          actual.Orientation);
		Assert.Equal(shipType,             actual.Type);
		Assert.Equal(startPosition,        actual.Position);
		Assert.Equal(expectedSegmentCount, actual.Segments.Count);
		Assert.Equal(startPosition,        actual.Segments.First().Key);
		Assert.Equal(lastPosition,         actual.Segments.Last().Key);
	}

	[Fact]
	public void AttackABattleship()
	{
		Ship ship = new(ShipType.Battleship, "A1", Orientation.Horizontal);

		Assert.False(ship.IsSunk);
		
		AttackResult result = ship.Attack("B1");
		Assert.Equal("B1", result.AttackCoordinate);
		Assert.Equal(AttackResultType.Miss, result.AttackResultType);
		Assert.False(ship.IsSunk);

		result = ship.Attack("A2");
		Assert.Equal(AttackResultType.Hit, result.AttackResultType);
		Assert.Equal(ShipType.Battleship, result.ShipType);
		Assert.False(ship.IsSunk);

		result = ship.Attack("A2");
		Assert.Equal(AttackResultType.Hit, result.AttackResultType);
		Assert.False(ship.IsSunk);

		result = ship.Attack("A1");
		Assert.Equal(AttackResultType.Hit, result.AttackResultType);
		Assert.False(ship.IsSunk);

		result = ship.Attack("A4");
		Assert.Equal(AttackResultType.Hit, result.AttackResultType);
		Assert.False(ship.IsSunk);

		result = ship.Attack("A3");
		Assert.Equal(AttackResultType.HitAndSunk, result.AttackResultType);
		Assert.True(ship.IsSunk);


	}

	[Fact]
	public void SerializeDeserializeBattleship()
	{
		Ship ship = new(ShipType.Battleship, new(1,1), Orientation.Horizontal);
		string expected = $$"""{"Type":{{(int)ship.Type}},"Position":null,"Orientation":{{(int)ship.Orientation}},"SerializedSegments":[],"NoOfSegments":{{ship.NoOfSegments}},"IsPositioned":false,"IsAfloat":false,"IsSunk":false}""";
		string actualJson = JsonSerializer.Serialize(ship);
		//Assert.Equal(expected, actualJson);

		Ship? actual = JsonSerializer.Deserialize<Ship>(actualJson);
		Assert.NotNull(actual);
		Assert.Equal(ship.Type,               actual.Type);
		Assert.Equal(ship.Position,           actual.Position);
		Assert.Equal(ship.Orientation,        actual.Orientation);
		Assert.Equal(ship.IsAfloat,           actual.IsAfloat);
		Assert.Equal(ship.IsPositioned,       actual.IsPositioned);
		Assert.Equal(ship.IsSunk,             actual.IsSunk);
		Assert.Equal(ship.NoOfSegments,       actual.NoOfSegments);
		Assert.Equal(ship.NoOfSegments,       actual.Segments.Count);
		Assert.Equal(ship.Segments.Count,     actual.Segments.Count);
		Assert.Equal(ship.Segments[new(1,1)], actual.Segments[new(1, 1)]);
		Assert.Equal(ship.Segments[new(1,2)], actual.Segments[new(1, 2)]);
		Assert.Equal(ship.Segments[new(1,3)], actual.Segments[new(1, 3)]);
		Assert.Equal(ship.Segments[new(1,4)], actual.Segments[new(1, 4)]);
	}


}
