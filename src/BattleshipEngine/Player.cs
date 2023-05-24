namespace BattleshipEngine;

public record Player(string Name)
{
	public PlayerId Id { get; set; } = PlayerId.Generate();

	public static Player PublicPlayer(Player player)
	{
		return player switch
		{
			AuthPlayer p => p with { PrivateId = default },
			ComputerPlayer => player,
			_ => player,
		};
	}
}

public record ComputerPlayer(string Name) : Player(Name);

public record AuthPlayer(string Name) : Player(Name)
{
	public PlayerId PrivateId { get; set; } = PlayerId.Generate();

	public bool IsUserWhoTheySayTheyAre(Player player) => player switch
	{
		AuthPlayer privatePlayer => privatePlayer.PrivateId == PrivateId,
		_ => false,
	};
}

