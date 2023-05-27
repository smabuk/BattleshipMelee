namespace BSMConsole.Themes;

internal class DefaultTheme : ITheme
{
	public string Name { get; set; } = "default";

	public string BackgroundColour { get; set; } = AnsiConsole.Background.ToMarkup();
	public string ForegroundColour { get; set; } = AnsiConsole.Foreground.ToMarkup();

	private string EmptyFgColour => "blue";
	private string HitFgColour => "red";
	private string MissFgColour => "blue";
	private string SunkFgColour => HitFgColour;
	private string WinnerFgColour => "gold1";

	public string Colour       => ITheme.GetColour(ForegroundColour, BackgroundColour);
	public string EmptyColour  => ITheme.GetColour(EmptyFgColour, BackgroundColour);
	public string HitColour    => ITheme.GetColour(HitFgColour, BackgroundColour);
	public string MissColour   => ITheme.GetColour(MissFgColour, BackgroundColour);
	public string SunkColour   => ITheme.GetColour(SunkFgColour, BackgroundColour);
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




