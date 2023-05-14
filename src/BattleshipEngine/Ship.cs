namespace BattleshipEngine;

public record Ship(ShipType Type)
{
	public Dictionary<Coordinate, ShipSegment> Segments { get; private set; } = new();

	public Ship(ShipType Type, Coordinate Position, Orientation Orientation) : this(Type)
	{
		this.Orientation = Orientation;
		this.Position = Position;
		foreach (ShipSegment segment in GetSegmentsWhenPlaced(Type, Position, Orientation)) {
			Segments.Add(segment.Coordinate, segment);
		}
	}

	public Orientation Orientation { get; }
	public Coordinate Position { get; }

	public bool IsPositioned => Segments.Any();
	public bool IsAfloat => IsPositioned && Segments.Values.Any(s => s.IsHit is false);
	public bool IsSunk => IsPositioned && Segments.Values.All(s => s.IsHit);

	public AttackResult Attack(Coordinate attackCoordinate)
	{
		if (Segments.ContainsKey(attackCoordinate)) {
			if (Segments[attackCoordinate].IsHit) {
				return new(attackCoordinate, IsSunk ? AttackResultType.HitAndSunk : AttackResultType.Hit, Type);
			}
			Segments[attackCoordinate] = Segments[attackCoordinate] with { IsHit = true };
			return new(attackCoordinate, IsSunk ? AttackResultType.HitAndSunk : AttackResultType.Hit, Type);
		}

		return new(attackCoordinate, AttackResultType.Miss);
	}

	private static List<ShipSegment> GetSegmentsWhenPlaced(ShipType type, Coordinate position, Orientation orientation)
	{

		List<ShipSegment> segments = new();

		int noOfSegments = type switch
		{
			ShipType.Battleship => 4,
			ShipType.Carrier    => 5,
			ShipType.Cruiser    => 3,
			ShipType.Destroyer  => 2,
			ShipType.Submarine  => 3,
			ShipType.RomulanBattleBagel => throw new NotImplementedException(),
			_ => throw new NotImplementedException(),
		};

		for (int i = 0; i < noOfSegments; i++) {
			Coordinate coord = orientation switch
			{
				Orientation.Vertical   => new(position.Row + i, position.Col),
				Orientation.Horizontal => new(position.Row,     position.Col + i),
				_ => throw new NotImplementedException(),
			};
			segments.Add(new(coord, false));
		}

		return segments;
	}

}
