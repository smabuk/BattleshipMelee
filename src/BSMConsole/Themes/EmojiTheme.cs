namespace BSMConsole.Themes;

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
	public string EmptyColour  => ITheme.GetColour(EmptyFgColour, BackgroundColour);
	public string HitColour    => ITheme.GetColour(HitFgColour, BackgroundColour);
	public string MissColour   => ITheme.GetColour(MissFgColour, BackgroundColour);
	public string SunkColour   => ITheme.GetColour(SunkFgColour, BackgroundColour);
	public string WinnerColour => ITheme.GetColour(WinnerFgColour, BackgroundColour);

	public string Empty => ".";
	public string Miss => "🌊";

	public bool CanIDisplayProperly() => ITheme.CanIDisplayEmojiProperly();

	public string GetShipNames(ShipType shipType)
	{
		return shipType switch
		{
			ShipType.AircraftCarrier    => "Yachts",
			ShipType.Battleship         => "Ferry",
			ShipType.Cruiser            => "Cruise liner",
			ShipType.Destroyer          => "Surfers",
			ShipType.RomulanBattleBagel => "Ufos",
			ShipType.Submarine          => "Rowing boat",
			_ => throw new NotImplementedException(),
		};
	}

	public string GetShipShape(ShipType shipType, bool IsSunk)
	{
		return shipType switch
		{
			_ when IsSunk               => "💣",
			ShipType.AircraftCarrier    => "⛵",
			ShipType.Battleship         => "⛴️",
			ShipType.Cruiser            => "🛳️",
			ShipType.Destroyer          => "🏄",
			ShipType.RomulanBattleBagel => "🛸",
			ShipType.Submarine          => "🚣",
			_ => throw new NotImplementedException(),
		};
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
}




