namespace BattleshipEngine;

[Serializable]
public record PlayerId()
{
	private Guid Id { get;  init; } = Guid.NewGuid();

	public override string ToString() => Id.ToString();

	public static implicit operator PlayerId(Guid guid) => new() { Id = guid } ;
	public static implicit operator string(PlayerId id) => id.ToString();
}
