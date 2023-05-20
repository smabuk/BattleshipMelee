namespace BattleshipEngine;

public abstract record Player(string Name)
{
	public Guid Id { get; internal init; } = Guid.NewGuid();

	public static Player PublicPlayer(Player player)
	{
		return player switch
		{
			PrivatePlayer p => p with { PrivateId = default },
			ComputerPlayer => player,
			_ => player,
		};
	}
}

public record ComputerPlayer(string Name) : Player(Name);

public record PrivatePlayer(string Name) : Player(Name)
{
	public Guid PrivateId { get; init; } = Guid.NewGuid();

	public bool IsUserWhoTheySayTheyAre(Player player) => player switch
	{
		PrivatePlayer privatePlayer => privatePlayer.PrivateId == PrivateId,
		_ => false,
	};
}

