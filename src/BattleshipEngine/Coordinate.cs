namespace BattleshipEngine;

public record Coordinate(int Row, int Col) : IParsable<Coordinate>
{
	const int MIN = 1;
	const int MAX = 10; // J
	const char MIN_LETTER = 'A';
	const char MAX_LETTER = 'J';

	public Coordinate(string coord) : this(Parse(coord).Row, Parse(coord).Col) { }

	public override string ToString() => IsValid ? $"{Convert.ToChar(Row - 1 + MIN_LETTER)}{Col}" : "";

	public static implicit operator Coordinate((int Row, int Col) coord) => new(coord.Row, coord.Col);
	public static implicit operator Coordinate(string input) => Parse(input, null);

	public static implicit operator (int Row, int Col)(Coordinate coord) => (coord.Row, coord.Col);
	public static implicit operator string(Coordinate coord) => coord.ToString();

	/// <summary>
	/// Calculates and index into the board array
	/// </summary>
	/// <param name="boardSize">The width and height of the board</param>
	/// <returns>The index to access the board array</returns>
	public int BoardIndex(int boardSize = 10) => ((Row - 1) * boardSize) + (Col - 1);

	public bool IsValid => (Row is >= MIN and <= MAX) && (Col is >= MIN and <= MAX);

	#region IParsable

	//[GeneratedRegex("""(?<rowLetter>[a-zA-Z])(?<columnNumber>\d+)""")]
	//private static partial Regex CoordinateRegex();

	public static Coordinate Parse(string? coordinate)
	{
		const string OUT_OF_RANGE_MESSAGE = $"{nameof(coordinate)} must start with a single letter and be followed by a number over 0 like D9.";

		ArgumentNullException.ThrowIfNull(coordinate, nameof(coordinate));
		if (coordinate.Length < 2) {
			throw new ArgumentOutOfRangeException(nameof(coordinate), OUT_OF_RANGE_MESSAGE);
		}

		char rowLetter = coordinate.ToUpperInvariant()[0];
		if (rowLetter is < MIN_LETTER or > MAX_LETTER) {
			throw new ArgumentOutOfRangeException(nameof(coordinate), OUT_OF_RANGE_MESSAGE);
		}

		int row = rowLetter - MIN_LETTER + 1;
		int col = int.Parse(coordinate[1..], NumberStyles.None);
		if (col <= 0) {
			throw new ArgumentOutOfRangeException(nameof(coordinate), OUT_OF_RANGE_MESSAGE);
		}
		return new Coordinate(row, col);
	}

	public static Coordinate Parse(string s, IFormatProvider? provider = null) => Parse(s);

	public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out Coordinate result)
	{
		if (s is null) {
			result = default;
			return false;
		}

		if (s.Length < 2) {
			result = default;
			return false;
		}

		char rowLetter = s.ToUpperInvariant()[0];
		if (rowLetter is < MIN_LETTER or > MAX_LETTER) {
			result = default;
			return false;
		}

		int row = rowLetter - MIN_LETTER + 1;
		bool success = int.TryParse(s[1..], null, out int col);
		if (success && col > 0 && (col is >= MIN and <= MAX)) {
			result = new Coordinate(row, col);
			return true;
		} else {
			result = default;
			return false;
		}
	}

	#endregion IParsable

}
