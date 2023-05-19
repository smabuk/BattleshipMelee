namespace BattleshipEngine;

public record Player(string Name, bool IsComputer = false)
{
	public Guid Id { get; internal init; } = Guid.NewGuid();

	public static Player PublicPlayer(Player player)
	{
		return player switch
		{
			PrivatePlayer p => new(p.Name, p.IsComputer) { Id = p.Id },
			_ => player,
		};
	}
}

public record PrivatePlayer(string Name, bool IsComputer = false) : Player(Name, IsComputer)
{
	public Guid PrivateId { get; init; } = Guid.NewGuid();

	public bool IsUserWhoTheySayTheyAre(Player player) => player switch
	{
		PrivatePlayer privatePlayer => privatePlayer.PrivateId == PrivateId,
		_ => false,
	};
}

