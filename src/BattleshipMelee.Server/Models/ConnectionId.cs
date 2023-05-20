namespace BattleshipMelee.Server.Models;

[Serializable]
public record ConnectionId
{
	private string Id { get; init; } = "";

	public static implicit operator ConnectionId(string id) => new() { Id = id };
	public static implicit operator string(ConnectionId id) => id.ToString() ?? "";
}
