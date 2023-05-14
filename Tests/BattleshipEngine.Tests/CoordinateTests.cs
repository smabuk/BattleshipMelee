namespace BattleshipEngine.Tests;

public sealed class CoordinateTests
{
	[Theory]
	[InlineData("A1", 1, 1)]
	[InlineData("A2", 1, 2)]
	[InlineData("A3", 1, 3)]
	[InlineData("J3", 10, 3)]
	[InlineData("J10", 10, 10)]
	public void ConvertFromCoordinateToString(string expected, int row, int col)
	{
		string actual = new Coordinate(row, col).ToString();
		Assert.Equal(expected, actual);
	}

	[Theory]
	[InlineData("A1", 1, 1)]
	[InlineData("A2", 1, 2)]
	[InlineData("A3", 1, 3)]
	[InlineData("J3", 10, 3)]
	[InlineData("J10", 10, 10)]
	[InlineData("a1", 1, 1)]
	[InlineData("j10", 10, 10)]
	public void ConvertFromStringToCoordinate(string input, int row, int col)
	{
		Coordinate expected = new Coordinate(row, col);
		Coordinate actual = input;
		Assert.Equal(expected, actual);
	}

	[Theory]
	[InlineData(1, 1)]
	[InlineData(1, 2)]
	[InlineData(1, 3)]
	[InlineData(10, 3)]
	[InlineData(10, 10)]
	public void ConvertFromTupleToCoordinate(int row, int col)
	{
		Coordinate expected = new Coordinate(row, col);
		Coordinate actual = (row, col);
		Assert.Equal(expected, actual);
		Assert.Equal(row, actual.Row);
		Assert.Equal(col, actual.Col);
	}

	[Theory]
	[InlineData("A1", 1, 1)]
	[InlineData("A2", 1, 2)]
	[InlineData("A3", 1, 3)]
	[InlineData("J3", 10, 3)]
	[InlineData("J10", 10, 10)]
	[InlineData("a1", 1, 1)]
	[InlineData("j10", 10, 10)]
	public void CreateCoordinateFromString(string input, int row, int col)
	{
		Coordinate expected = new Coordinate(row, col);
		Coordinate actual = new(input);
		Assert.Equal(expected, actual);
	}


	[Theory]
	[InlineData(01, 01, 00)]
	[InlineData(01, 02, 01)]
	[InlineData(01, 03, 02)]
	[InlineData(10, 03, 92)]
	[InlineData(10, 10, 99)]
	public void IndexFromCoordinate(int row, int col, int expected)
	{
		int actual = new Coordinate(row, col).BoardIndex();
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void ThrowIfInvalid()
	{
		const string OUT_OF_RANGE_MESSAGE = "coordinate must start with a single letter and be followed by a number over 0 like D9. (Parameter 'coordinate')";

		Assert.Throws<ArgumentNullException>(() => { Coordinate actual = new Coordinate(null!); });

		Exception exception;
		exception = Assert.Throws<ArgumentOutOfRangeException>(() => { Coordinate actual = new Coordinate(""); });
		Assert.Equal(OUT_OF_RANGE_MESSAGE, ((ArgumentOutOfRangeException)exception).Message);

		exception = Assert.Throws<ArgumentOutOfRangeException>(() => { Coordinate actual = new Coordinate("12"); });
		Assert.Equal(OUT_OF_RANGE_MESSAGE, ((ArgumentOutOfRangeException)exception).Message);

		exception = Assert.Throws<ArgumentOutOfRangeException>(() => { Coordinate actual = new Coordinate("A0"); });
		Assert.Equal(OUT_OF_RANGE_MESSAGE, ((ArgumentOutOfRangeException)exception).Message);

		exception = Assert.Throws<FormatException>(() => { Coordinate actual = new Coordinate("AZ"); });
		Assert.Equal("The input string 'Z' was not in a correct format.", ((FormatException)exception).Message);

		exception = Assert.Throws<FormatException>(() => { Coordinate actual = new Coordinate("A-3"); });
		Assert.Equal("The input string '-3' was not in a correct format.", ((FormatException)exception).Message);
	}

}
