using BattleshipEngine;

namespace BSMConsole;
internal interface ITheme
{
	public string BackgroundColour { get; set; }
	public string ForegroundColour { get; set; }

	public string Colour => GetColour(ForegroundColour, BackgroundColour);

	public string EmptyColour => "blue";
	public string HitColour   => "red";
	public string MissColour   => "blue";
	public string SunkColour => HitColour;
	public string WinnerColour => "gold1";

	string Name  => "Default";
	string Empty => ".";
	string Miss  => "O";

	public string GetShipNames(ShipType shipType) => shipType.ToFriendlyString();

	string GetShipShape(ShipType shipType, bool IsSunk)
	{
		return IsSunk
			? shipType.ToString()[0].ToString().ToUpperInvariant()
			: shipType.ToString()[0].ToString().ToLowerInvariant();
	}

	internal static string GetColour(string fg, string bg) => $"{fg.Trim()} on {bg.Trim()}";
}
