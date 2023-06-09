﻿namespace BattleshipEngine;

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
	public static string ToFriendlyString(this ShipType shipType) => shipType switch
	{
		ShipType.AircraftCarrier    => "Aircraft Carrier",
		ShipType.RomulanBattleBagel => "Romulan Battle Bagel",
		_                           => $"{shipType}",
	};
}
