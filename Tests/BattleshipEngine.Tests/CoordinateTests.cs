﻿namespace BattleshipEngine.Tests;

public sealed class CoordinateTests
{
	[Theory]
	[InlineData("A1",   1,  1)]
	[InlineData("A2",   1,  2)]
	[InlineData("A3",   1,  3)]
	[InlineData("J3",  10,  3)]
	[InlineData("J10", 10, 10)]
	public void ConvertFromCoordinateToString(string expected, int row, int col)
	{
		string actual = new Coordinate(row, col).ToString();
		Assert.Equal(expected, actual);
	}

	[Theory]
	[InlineData("A1",   1,  1)]
	[InlineData("A2",   1 , 2)]
	[InlineData("A3",   1,  3)]
	[InlineData("J3",  10,  3)]
	[InlineData("J10", 10, 10)]
	[InlineData("a1",   1,  1)]
	[InlineData("j10", 10, 10)]
	public void ConvertFromStringToCoordinate(string input, int row, int col)
	{
		Coordinate expected = new Coordinate(row, col);
		Coordinate actual = input;
		Assert.Equal(expected, actual);
	}

	[Theory]
	[InlineData( 1,  1)]
	[InlineData( 1,  2)]
	[InlineData( 1,  3)]
	[InlineData(10,  3)]
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
	[InlineData("A1",   1,  1)]
	[InlineData("A2",   1,  2)]
	[InlineData("A3",   1,  3)]
	[InlineData("J3",  10,  3)]
	[InlineData("J10", 10, 10)]
	[InlineData("a1",   1,  1)]
	[InlineData("j10", 10, 10)]
	public void CreateCoordinateFromString(string input, int row, int col)
	{
		Coordinate expected = new Coordinate(row, col);
		Coordinate actual = input;
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

	[Theory]
	[InlineData( 1,  1)]
	[InlineData(-1,  2, false)]
	[InlineData(10, 10)]
	[InlineData(11, 11, false)]
	public void SerializeCoordinate(int row, int col, bool isValid = true)
	{
		Coordinate coordinate = new Coordinate(row, col);
		string expected = $$"""{"Row":{{row}},"Col":{{col}},"IsValid":{{isValid.ToString().ToLowerInvariant()}}}""";
		string actual = JsonSerializer.Serialize(coordinate);
		Assert.Equal(expected, actual);
	}

	[Theory]
	[InlineData( 1,  1)]
	[InlineData(10, 10)]
	[InlineData(-1,  2, false)]
	[InlineData( 1, -2, false)]
	[InlineData(11, 11, false)]
	public void DeserializeCoordinate(int row, int col, bool isValid = true)
	{
		Coordinate expected = new Coordinate(row, col);
		string json = $$"""{"Row":{{row}},"Col":{{col}},"IsValid":{{isValid.ToString().ToLowerInvariant()}}}""";
		Coordinate? actual = JsonSerializer.Deserialize<Coordinate>(json);
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void ThrowIfInvalid()
	{
		const string OUT_OF_RANGE_MESSAGE = "coordinate must start with a single letter and be followed by a number over 0 like D9. (Parameter 'coordinate')";

		//Assert.Throws<ArgumentNullException>(() => { Coordinate actual = new Coordinate(null!); });

		Exception exception;
		exception = Assert.Throws<ArgumentOutOfRangeException>(() => { Coordinate actual = ""; });
		Assert.Equal(OUT_OF_RANGE_MESSAGE, ((ArgumentOutOfRangeException)exception).Message);

		exception = Assert.Throws<ArgumentOutOfRangeException>(() => { Coordinate actual = "12"; });
		Assert.Equal(OUT_OF_RANGE_MESSAGE, ((ArgumentOutOfRangeException)exception).Message);

		exception = Assert.Throws<ArgumentOutOfRangeException>(() => { Coordinate actual = "A0"; });
		Assert.Equal(OUT_OF_RANGE_MESSAGE, ((ArgumentOutOfRangeException)exception).Message);

		exception = Assert.Throws<FormatException>(() => { Coordinate actual = "AZ"; });
		Assert.Equal("The input string 'Z' was not in a correct format.", ((FormatException)exception).Message);

		exception = Assert.Throws<FormatException>(() => { Coordinate actual = "A-3"; });
		Assert.Equal("The input string '-3' was not in a correct format.", ((FormatException)exception).Message);
	}

	[Theory]
	[InlineData("A1",   1,  1, true)]
	[InlineData("A2",   1,  2, true)]
	[InlineData("A3",   1,  3, true)]
	[InlineData("J3",  10,  3, true)]
	[InlineData("J10", 10, 10, true)]
	[InlineData("a1",   1,  1, true)]
	[InlineData("j10", 10, 10, true)]
	[InlineData("q1",   0,  0, false)]
	[InlineData("k1",   0,  0, false)]
	[InlineData("j11",  0,  0, false)]
	[InlineData("j-3",  0,  0, false)]
	[InlineData("-3",   0,  0, false)]
	public void TryToParseAndReturnCoordinate(string input, int row, int col, bool expected)
	{
		bool actual = Coordinate.TryParse(input, null, out Coordinate? coordinate);
		Assert.Equal(expected, actual);
		if (actual) {
			Assert.Equal(new(row, col), coordinate);
		}
	}

}
