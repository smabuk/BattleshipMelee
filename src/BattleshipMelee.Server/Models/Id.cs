namespace BattleshipMelee.Server.Models;

[Serializable]
public class IdType<T>
{
	private string Id { get; init; } = "";

	public static implicit operator IdType<T>(string id) => new() { Id = id };
	public static implicit operator string(IdType<T> id) => id.ToString() ?? "";
}
