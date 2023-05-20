using System.Text.Json.Serialization;

namespace BattleshipEngine;

public record Ship(ShipType Type, Coordinate Position = null! , Orientation Orientation = Orientation.Horizontal)
{
	// See https://stackoverflow.com/questions/24504245/not-ableto-serialize-dictionary-with-complex-key-using-json-net
	[JsonIgnore]
	public Dictionary<Coordinate, ShipSegment> Segments { get; private set; } = Position is null ? new() : Ship
		.GetSegmentsWhenPlaced(Type, Position, Orientation)
		.ToDictionary(x => x.Coordinate, x => x);

	[JsonInclude]
	public List<ShipSegment> SerializedSegments
	{
		get { return Segments.Values.ToList(); }
		set { Segments = value.ToDictionary(x => x.Coordinate, x => x); }
	}

	public int NoOfSegments => GetNoOfSegments(Type);
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
		if (type is ShipType.RomulanBattleBagel) {
			return GetRomulanBattleBagelSegments();
		}

		List<ShipSegment> segments = new();

		int noOfSegments = GetNoOfSegments(type);

		for (int i = 0; i < noOfSegments; i++) {
			Coordinate coord = orientation switch
			{
				Orientation.Vertical => new(position.Row + i, position.Col),
				Orientation.Horizontal => new(position.Row, position.Col + i),
				_ => throw new NotImplementedException(),
			};
			segments.Add(new(coord, false));
		}

		return segments;

		List<ShipSegment> GetRomulanBattleBagelSegments()
		{
			List<ShipSegment> segments = new();
			if (orientation is Orientation.Horizontal) {
				segments.Add(new(new(position.Row + 0, position.Col + 0), false));
				segments.Add(new(new(position.Row - 1, position.Col + 1), false));
				segments.Add(new(new(position.Row + 1, position.Col + 1), false));
				segments.Add(new(new(position.Row + 0, position.Col + 2), false));
			} else {
				segments.Add(new(new(position.Row + 0, position.Col + 0), false));
				segments.Add(new(new(position.Row + 1, position.Col - 1), false));
				segments.Add(new(new(position.Row + 1, position.Col + 1), false));
				segments.Add(new(new(position.Row + 2, position.Col + 0), false));
			}
			return segments;
		}
	}

	public static int GetNoOfSegments(ShipType type) => type switch
	{
		ShipType.Battleship => 4,
		ShipType.AircraftCarrier => 5,
		ShipType.Cruiser => 3,
		ShipType.Destroyer => 2,
		ShipType.Submarine => 3,
		ShipType.RomulanBattleBagel => 4,
		_ => throw new NotImplementedException(),
	};

}
