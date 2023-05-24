using BattleshipEngine;

using Spectre.Console;

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

	public bool CanIDisplayProperly() => true;

	public string GetShipNames(ShipType shipType) => shipType.ToFriendlyString();

	public string GetShipShape(ShipType shipType, bool IsSunk)
	{
		return IsSunk
			? shipType.ToString()[0].ToString().ToUpperInvariant()
			: shipType.ToString()[0].ToString().ToLowerInvariant();
	}

	public string GetStatusMessages(GameStatus gameStatus)
	{
		return gameStatus switch
		{
			GameStatus.PlacingShips  => "Place your ships",
			GameStatus.AddingPlayers => "Adding players",
			GameStatus.Attacking     => "Attack those ships",
			GameStatus.GameOver      => "GAME OVER",
			GameStatus.Abandoned     => "Abandoned",
			_ => ""
		};
	}

	internal static string GetColour(string fg, string bg) => $"{fg.Trim()} on {bg.Trim()}";

	internal static bool CanIDisplayEmojiProperly()
	{
		// Simplistic hack to make sure it is not using 437 or similar
		return Console.OutputEncoding.CodePage > 1000;
	}

}
