namespace BattleshipEngine;

public record struct Coordinate(int Row, int Col) : IParsable<Coordinate>
{
	public Coordinate(string coord) : this(Parse(coord).Row, Parse(coord).Col)
	{
	}

	public override readonly string ToString() => IsValid ? $"{Convert.ToChar(Row - 1 + 'A')}{Col}" : "";

	public static implicit operator Coordinate((int Row, int Col) coord) => new(coord.Row, coord.Col);
	public static implicit operator Coordinate(string input) => Parse(input, null);

	public static implicit operator (int Row, int Col)(Coordinate coord) => (coord.Row, coord.Col);
	public static implicit operator string(Coordinate coord) => coord.ToString();

	/// <summary>
	/// Calculates and index into the board array
	/// </summary>
	/// <param name="boardSize">The width and height of the board</param>
	/// <returns>The index to access the board array</returns>
	public readonly int BoardIndex(int boardSize = 10) => ((Row - 1) * boardSize) + (Col - 1);

	public readonly bool IsValid => (Row is > 0 and <= 26) && (Col is > 0 and <= 26);

	#region IParsable

	//[GeneratedRegex("""(?<rowLetter>[a-zA-Z])(?<columnNumber>\d+)""")]
	//private static partial Regex CoordinateRegex();

	public static Coordinate Parse(string coordinate)
	{
		const string OUT_OF_RANGE_MESSAGE = $"{nameof(coordinate)} must start with a single letter and be followed by a number over 0 like D9.";

		ArgumentNullException.ThrowIfNull(coordinate, nameof(coordinate));
		if (coordinate.Length < 2) {
			throw new ArgumentOutOfRangeException(nameof(coordinate), OUT_OF_RANGE_MESSAGE);
		}

		char rowLetter = coordinate.ToUpperInvariant()[0];
		if (rowLetter is < 'A' or > 'Z') {
			throw new ArgumentOutOfRangeException(nameof(coordinate), OUT_OF_RANGE_MESSAGE);
		}

		int row = rowLetter - 'A' + 1;
		int col = int.Parse(coordinate[1..], NumberStyles.None);
		if (col <= 0) {
			throw new ArgumentOutOfRangeException(nameof(coordinate), OUT_OF_RANGE_MESSAGE);
		}
		return new Coordinate(row, col);
	}

	public static Coordinate Parse(string coordinate, IFormatProvider? provider) => Parse(coordinate);

	public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out Coordinate result)
	{
		if (s is null) {
			result = null!;
			return false;
		}

		if (s.Length < 2) {
			result = null!;
			return false;
		}

		char rowLetter = s.ToUpperInvariant()[0];
		if (rowLetter is < 'A' or > 'Z') {
			result = null!;
			return false;
		}

		int row = rowLetter - 'A' + 1;
		bool success = int.TryParse(s[1..], null, out int col);
		if (success && col > 0) {
			result = new Coordinate(row, col);
			return true;
		} else {
			result = null!;
			return false;
		}
	}

	#endregion IParsable

}
