namespace BattleshipEngine;

public readonly record struct PlayerId()
{
	private Guid Id { get;  init; }

	public override readonly string ToString() => Id.ToString();

	public static implicit operator PlayerId(Guid guid) => new() { Id = guid } ;
	public static implicit operator string(PlayerId id) => id.ToString();

}
