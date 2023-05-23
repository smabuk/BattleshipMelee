namespace BattleshipEngine;

[StronglyTypedId(converters: StronglyTypedIdConverter.TypeConverter | StronglyTypedIdConverter.SystemTextJson)]
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public partial struct PlayerId : IParsable<PlayerId>
{
	public static PlayerId Generate() => new(Guid.NewGuid());

	public static PlayerId Parse(string s, IFormatProvider? provider) => new(Guid.Parse(s));

	public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out PlayerId result)
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






//[Serializable]
//public record PlayerId()
//{
//	private Guid Id { get; set; } = Guid.NewGuid();

//	public override string ToString() => Id.ToString();

//	public static implicit operator PlayerId(Guid guid) => new() { Id = guid };
//	public static implicit operator string(PlayerId id) => id.ToString();
//}
