namespace BattleshipEngine;

public record class Player(string Name, bool IsComputer = false)
{
	public PlayerId Id { get; internal init; } = Guid.NewGuid();

	public static Player PublicPlayer(Player player)
	{
		return player switch
		{
			PrivatePlayer p => new(p.Name, p.IsComputer) { Id = p.Id },
			_ => player,
		};
	}
}

public record class PrivatePlayer(string Name, bool IsComputer = false) : Player(Name, IsComputer)
{
	public PlayerId PrivateId { get; internal init; } = Guid.NewGuid();

	public bool IsUserWhoTheySayTheyAre(Player player) => player switch
	{
		PrivatePlayer privatePlayer => privatePlayer.PrivateId == PrivateId,
		_ => false,
	};
}

