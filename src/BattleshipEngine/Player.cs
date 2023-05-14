namespace BattleshipEngine;

public record struct Player(string Name, bool IsComputer = false)
{
	public Guid Id { get; private set; } = Guid.NewGuid();
}
