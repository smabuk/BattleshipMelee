namespace BattleshipMelee.Server.Models;

[Serializable]
public record GameId
{
	private Guid Id { get; init; } = Guid.NewGuid();

	public override string ToString() => Id.ToString();

	public static implicit operator GameId(Guid guid) => new() { Id = guid };
	public static implicit operator string(GameId id) => id.ToString();
}
