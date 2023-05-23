using BattleshipEngine;

namespace BSMConsole;

internal class DefaultTheme : ITheme
{
	public string Name { get; set; } = "default";

	public string BackgroundColour { get; set; } = AnsiConsole.Background.ToMarkup();
	public string ForegroundColour { get; set; } = AnsiConsole.Foreground.ToMarkup();

	private string EmptyFgColour  => "blue";
	private string HitFgColour    => "red";
	private string MissFgColour   => "blue";
	private string SunkFgColour   => HitFgColour;
	private string WinnerFgColour => "gold1";

	public string Colour       => ITheme.GetColour(ForegroundColour, BackgroundColour);
	public string EmptyColour  => ITheme.GetColour(EmptyFgColour,  BackgroundColour);
	public string HitColour    => ITheme.GetColour(HitFgColour,    BackgroundColour);
	public string MissColour   => ITheme.GetColour(MissFgColour,   BackgroundColour);
	public string SunkColour   => ITheme.GetColour(SunkFgColour,   BackgroundColour);
	public string WinnerColour => ITheme.GetColour(WinnerFgColour, BackgroundColour);

	public string Empty => ".";
	public string Miss  => "O";

	public string GetShipNames(ShipType shipType) => shipType.ToFriendlyString();

	public string GetShipShape(ShipType shipType, bool IsSunk)
	{
		return IsSunk
			? shipType.ToString()[0].ToString().ToUpperInvariant()
			: shipType.ToString()[0].ToString().ToLowerInvariant();
	}
}

internal class EmojiTheme : ITheme
{
	public string Name => "emoji";

	public string BackgroundColour { get; set; } = "black";
	public string ForegroundColour { get; set; } = "silver";

	private string EmptyFgColour  => "blue";
	private string HitFgColour    => "red";
	private string MissFgColour   => "blue";
	private string SunkFgColour   => HitFgColour;
	private string WinnerFgColour => "gold1";

	public string Colour       => ITheme.GetColour(ForegroundColour, BackgroundColour);
	public string EmptyColour  => ITheme.GetColour(EmptyFgColour,  BackgroundColour);
	public string HitColour    => ITheme.GetColour(HitFgColour,    BackgroundColour);
	public string MissColour   => ITheme.GetColour(MissFgColour,   BackgroundColour);
	public string SunkColour   => ITheme.GetColour(SunkFgColour,   BackgroundColour);
	public string WinnerColour => ITheme.GetColour(WinnerFgColour, BackgroundColour);

	public string Empty => ":water_wave:";
	public string Miss  => ":water_wave:";

	public string GetShipNames(ShipType shipType)
	{
		return shipType.ToFriendlyString();
	}

	public string GetShipShape(ShipType shipType, bool IsSunk = false)
	{
		return IsSunk
			? shipType.ToString()[0].ToString().ToUpperInvariant()
			: shipType.ToString()[0].ToString().ToLowerInvariant();
	}
}

internal class AnimalsTheme : ITheme
{
	public string Name => "animals";

	public string BackgroundColour { get; set; } = "black";
	public string ForegroundColour { get; set; } = "green";

	private string EmptyFgColour  => "green";
	private string HitFgColour    => "red";
	private string MissFgColour   => "blue";
	private string SunkFgColour   => HitFgColour;
	private string WinnerFgColour => "gold1";

	public string Colour       => ITheme.GetColour(ForegroundColour, BackgroundColour);
	public string EmptyColour  => ITheme.GetColour(EmptyFgColour,  BackgroundColour);
	public string HitColour    => ITheme.GetColour(HitFgColour,    BackgroundColour);
	public string MissColour   => ITheme.GetColour(MissFgColour,   BackgroundColour);
	public string SunkColour   => ITheme.GetColour(SunkFgColour,   BackgroundColour);
	public string WinnerColour => ITheme.GetColour(WinnerFgColour, BackgroundColour);

	public string Empty => ".";
	public string Miss  => ":evergreen_tree:";
	//public string Miss  => Random.Shared.Next(0, 4) switch
	//{
	//	0 => ":deciduous_tree:",
	//	1 => ":palm_tree:",
	//	2 => ":christmas_tree:",
	//	3 => ":evergreen_tree:",
	//	_ => ":deciduous_tree:",
	//};

	public string GetShipNames(ShipType shipType)
	{
		return shipType switch
		{
			ShipType.AircraftCarrier    => "Horse",
			ShipType.Battleship         => "Tiger",
			ShipType.Cruiser            => "Dragon",
			ShipType.Destroyer          => "Cat",
			ShipType.RomulanBattleBagel => "Cow",
			ShipType.Submarine          => "Monkey",
			_ => throw new NotImplementedException(),
		};
	}

	public string GetShipShape(ShipType shipType, bool IsSunk)
	{
		return shipType switch
		{
			ShipType.AircraftCarrier    when IsSunk => ":horse_face:",
			ShipType.AircraftCarrier                => ":horse:",
			ShipType.Battleship         when IsSunk => ":tiger_face:",
			ShipType.Battleship                     => ":tiger:",
			ShipType.Cruiser            when IsSunk => ":dragon_face:",
			ShipType.Cruiser                        => ":dragon:",
			ShipType.Destroyer          when IsSunk => ":lion:",
			ShipType.Destroyer                      => ":cat:",
			ShipType.RomulanBattleBagel when IsSunk => ":cow_face:",
			ShipType.RomulanBattleBagel             => ":cow:",
			ShipType.Submarine          when IsSunk => ":monkey_face:",
			ShipType.Submarine                      => ":monkey:",
			_ => throw new NotImplementedException(),
		};
	}
}


