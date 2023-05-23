using System.Diagnostics;

using StronglyTypedIds;

namespace BattleshipEngine;

[StronglyTypedId(converters: StronglyTypedIdConverter.TypeConverter | StronglyTypedIdConverter.SystemTextJson)]
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public partial struct GameId : IParsable<GameId>
{
	public static GameId Generate() => new GameId(Guid.NewGuid());

	public static GameId Parse(string s, IFormatProvider? provider) => new(Guid.Parse(s));

	public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out GameId result)
	{
		result = default;
		bool success = Guid.TryParse(s, provider, out Guid guidResult);
		if (success) {
			result = new(guidResult);
			return true;
		}
		return false;
	}

	private string GetDebuggerDisplay()
	{
		return ToString();
	}
}










////[Serializable]
////public record GameId
////{
////	private Guid Id { get; init; } = Guid.NewGuid();

////	public override string ToString() => Id.ToString();

////	public static implicit operator GameId(Guid guid) => new() { Id = guid };
////	public static implicit operator string(GameId id) => id.ToString();
////}
