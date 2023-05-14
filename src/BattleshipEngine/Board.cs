[assembly: InternalsVisibleToAttribute("BattleshipEngine.Tests")]
namespace BattleshipEngine;

internal record Board(GameType GameType = GameType.Classic)
{
	private List<Ship> fleet = new();
	private HashSet<Coordinate> placedSegments = new();

	public void Init()
	{
		fleet = new();
		placedSegments = new();

		fleet = GameType switch
		{
			GameType.Classic => new()
			{
				new(ShipType.Destroyer),
				new(ShipType.Submarine),
				new(ShipType.Cruiser),
				new(ShipType.Battleship),
				new(ShipType.Carrier),
			},
			GameType.BigBangTheory => new()
			{
				new(ShipType.Destroyer),
				new(ShipType.Submarine),
				new(ShipType.Cruiser),
				new(ShipType.RomulanBattleBagel),
				new(ShipType.Carrier),
			},
			GameType.Melee => throw new NotImplementedException(),
			_ => throw new NotImplementedException(),
		};
	}

	public bool PlaceShip(Ship shipToPlace)
	{
		bool result = false;

		for (int fleetIndex = 0; fleetIndex < fleet.Count; fleetIndex++) {
			Ship ship = fleet[fleetIndex];
			if (ship.IsPositioned is false && ship.Type == shipToPlace.Type) {
				foreach (Coordinate coordinate in shipToPlace.Segments.Keys) {
					if (coordinate.Row > BoardSize || coordinate.Col > BoardSize) {
						return false;
					}
					if (placedSegments.Contains(coordinate)) {
						return false;
					}
				}
				fleet[fleetIndex] = shipToPlace;
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
		foreach (Ship ship in fleet) {
			AttackResult result = ship.Attack(attackCoordinate);
			if (result.HitOrMiss is AttackResultType.Hit or AttackResultType.HitAndSunk) {
				return result;
			}
		}
		return new(attackCoordinate, AttackResultType.Miss);
	}

	public bool IsFleetReady => !fleet.Any(ship => ship.IsPositioned == false);
	public bool IsFleetSunk => fleet.All(ship => ship.IsSunk);
	public List<Ship> Fleet => fleet.ToList();

	public int BoardSize => GameType switch
	{
		GameType.Classic => 10,
		GameType.Melee => throw new NotImplementedException(),
		GameType.BigBangTheory => throw new NotImplementedException(),
		_ => throw new NotImplementedException(),
	};

}
