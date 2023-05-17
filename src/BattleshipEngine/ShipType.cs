namespace BattleshipEngine;

public enum ShipType
{
	Battleship,
	[Description("Aircraft Carrier")]
	AircraftCarrier,
	Cruiser,
	Destroyer,
	Submarine,
	RomulanBattleBagel,
}

public static class ShipTypeExtensions
{
	public static string ToFriendlyString(this ShipType shipType)
	{
		return shipType switch
		{
			ShipType.AircraftCarrier => "Aircraft Carrier",
			_ => $"{shipType}",
		};
	}
}
