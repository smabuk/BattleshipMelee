[assembly: InternalsVisibleToAttribute("BattleshipEngine.Tests")]
namespace BattleshipEngine;

public record Board(int BoardSize)
{
	public required List<Ship> Fleet { get; init; }
	private readonly HashSet<Coordinate> placedSegments = new();

	public bool PlaceShip(Ship shipToPlace)
	{
		bool result = false;

		for (int fleetIndex = 0; fleetIndex < Fleet.Count; fleetIndex++) {
			Ship ship = Fleet[fleetIndex];
			if (ship.IsPositioned is false && ship.Type == shipToPlace.Type) {
				foreach (Coordinate coordinate in shipToPlace.Segments.Keys) {
					if (coordinate.Row < 1 || coordinate.Row > BoardSize || coordinate.Col < 1 || coordinate.Col > BoardSize) {
						return false;
					}
					if (placedSegments.Contains(coordinate)) {
						return false;
					}
				}
				Fleet[fleetIndex] = shipToPlace;
				foreach (Coordinate coordinate in shipToPlace.Segments.Keys) {
					placedSegments.Add(coordinate);
				}
				result = true;
			}
		}

		return result;
	}

	public AttackResult Attack(Coordinate attackCoordinate)
	{
		foreach (Ship ship in Fleet) {
			AttackResult result = ship.Attack(attackCoordinate);
			if (result.HitOrMiss is AttackResultType.Hit or AttackResultType.HitAndSunk) {
				return result;
			}
		}
		return new(attackCoordinate, AttackResultType.Miss);
	}

	public bool IsFleetReady => !Fleet.Any(ship => ship.IsPositioned == false);
	public bool IsFleetSunk => Fleet.All(ship => ship.IsSunk);

	public static bool ValidateShipPositions(IEnumerable<Ship> ships)
	{
		return !ships.SelectMany(ship => ship.Segments.Keys).GroupBy(c => c).Any(c => c.Count() > 1);
	}

}
