namespace BattleshipEngine;

public abstract record Player(string Name)
{
	public PlayerId Id { get; set; } = Guid.NewGuid().ToString();

	public static Player PublicPlayer(Player player)
	{
		return player switch
		{
			PrivatePlayer p => p with { PrivateId = "" },
			ComputerPlayer => player,
			_ => player,
		};
	}
}

public record ComputerPlayer(string Name) : Player(Name);

public record PrivatePlayer(string Name) : Player(Name)
{
	public PlayerId PrivateId { get; set; } = Guid.NewGuid().ToString();

	public bool IsUserWhoTheySayTheyAre(Player player) => player switch
	{
		PrivatePlayer privatePlayer => privatePlayer.PrivateId == PrivateId,
		_ => false,
	};
}

