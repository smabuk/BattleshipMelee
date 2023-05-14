namespace BattleshipEngine;

public record struct Player(string Name, bool IsComputer = false)
{
	public PlayerId Id { get; private set; } = Guid.NewGuid();
}
