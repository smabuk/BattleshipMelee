namespace BSMConsole.Themes;

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
	public string EmptyColour  => ITheme.GetColour(EmptyFgColour, BackgroundColour);
	public string HitColour    => ITheme.GetColour(HitFgColour, BackgroundColour);
	public string MissColour   => ITheme.GetColour(MissFgColour, BackgroundColour);
	public string SunkColour   => ITheme.GetColour(SunkFgColour, BackgroundColour);
	public string WinnerColour => ITheme.GetColour(WinnerFgColour, BackgroundColour);

	public string Empty => ".";
	public string Miss => ":evergreen_tree:";
	//public string Miss  => Random.Shared.Next(0, 4) switch
	//{
	//	0 => ":deciduous_tree:",
	//	1 => ":palm_tree:",
	//	2 => ":christmas_tree:",
	//	3 => ":evergreen_tree:",
	//	_ => ":deciduous_tree:",
	//};

	public bool CanIDisplayProperly() => ITheme.CanIDisplayEmojiProperly();

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

	public string GetStatusMessages(GameStatus gameStatus)
	{
		return gameStatus switch
		{
			GameStatus.PlacingShips  => "Place your animals",
			GameStatus.AddingPlayers => "Adding players",
			GameStatus.Attacking     => "Look for those animals",
			GameStatus.GameOver      => "GAME OVER",
			GameStatus.Abandoned     => "Abandoned",
			_ => ""
		};
	}
}




