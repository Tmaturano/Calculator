using AwesomeAssertions;
using Calculator.Core.Parsers;

namespace Calculator.Core.Tests;

public class StringCalculatorParserTests
{
    private readonly StringCalculatorParser _parser;

    public StringCalculatorParserTests()
    {
        _parser = new StringCalculatorParser();
    }

    [Theory]
    [InlineData("", 1)] // Should return [0]
    [InlineData("1", 1)]
    [InlineData("1,2", 2)]
    public void Parse_ValidInput_ReturnsCorrectNumberOfNumbers(string input, int expectedCount)
    {
        // Act
        var result = _parser.Parse(input);

        // Assert
        result.Should().NotBeNull();
        result.Numbers.Should().HaveCount(expectedCount);
    }

    [Theory]
    [InlineData("1", new[] { 1 })]
    [InlineData("1,2", new[] { 1, 2 })]
    [InlineData("4,-3", new[] { 4, -3 })]
    [InlineData("5,tytyt", new[] { 5, 0 })]
    [InlineData("a,10", new[] { 0, 10 })]
    public void Parse_ValidInput_ReturnsCorrectNumbers(string input, int[] expectedNumbers)
    {
        // Act
        var result = _parser.Parse(input);

        // Assert
        result.Should().NotBeNull();
        result.Numbers.Should().Equal(expectedNumbers);
    }

    [Fact]
    public void Parse_NullInput_ReturnsZero()
    {
        // Arrange
        string? input = null;

        // Act
        var result = _parser.Parse(input!);

        // Assert
        result.Should().NotBeNull();
        result.Numbers.Should().HaveCount(1);
        result.Numbers[0].Should().Be(0);
    }

    [Theory]
    [InlineData("1\n2,3", new[] { 1, 2, 3 })]
    [InlineData("1\n2\n3", new[] { 1, 2, 3 })]
    [InlineData("10\n20,30", new[] { 10, 20, 30 })]
    [InlineData("1\n2,3\n4,5\n6", new[] { 1, 2, 3, 4, 5, 6 })]
    [InlineData("\n1,2", new[] { 1, 2 })] // Leading newline
    [InlineData("1,2\n", new[] { 1, 2 })] // Trailing newline
    [InlineData("1\n\n2", new[] { 1, 2 })] // Multiple newlines
    [InlineData("", new[] { 0 })] // Empty input
    [InlineData("\n", new int[] { })] // Only newlines (empty after split)
    public void Parse_WithNewlineDelimiter_ReturnsCorrectNumbers(string input, int[] expectedNumbers)
    {
        // Act
        var result = _parser.Parse(input);

        // Assert
        result.Should().NotBeNull();
        result.Numbers.Should().Equal(expectedNumbers);
    }

    [Theory]
    [InlineData("1\nabc,3", new[] { 1, 0, 3 })]
    [InlineData("10\ntytyt\n20", new[] { 10, 0, 20 })]
    [InlineData("a\nb\nc", new[] { 0, 0, 0 })]
    public void Parse_WithNewlinesAndInvalidNumbers_ConvertsToZero(string input, int[] expectedNumbers)
    {
        // Act
        var result = _parser.Parse(input);

        // Assert
        result.Should().NotBeNull();
        result.Numbers.Should().Equal(expectedNumbers);
    }

    [Fact]
    public void Parse_ComplexMixedDelimiters_HandlesCorrectly()
    {
        // Arrange
        var input = "1\n2,3\n4,5\n6,7\n8,9\n10";

        // Act
        var result = _parser.Parse(input);

        // Assert
        result.Should().NotBeNull();
        result.Numbers.Should().HaveCount(10);
        result.Numbers.Should().Equal(1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
    }

    [Fact]
    public void Parse_WithOnlyNewlines_ReturnsEmptyList()
    {
        // Arrange
        var input = "\n\n\n\n";

        // Act
        var result = _parser.Parse(input);

        // Assert
        result.Should().NotBeNull();
        result.Numbers.Should().BeEmpty(); // StringSplitOptions.RemoveEmptyEntries removes all
    }

    [Theory]
    [InlineData("2,1001,6", new[] { 2, 0, 6 })]
    [InlineData("1000,1001", new[] { 1000, 0 })]
    [InlineData("1001,1002,1003", new[] { 0, 0, 0 })]
    [InlineData("500,1500,2000,300", new[] { 500, 0, 0, 300 })]
    [InlineData("999,1000,1001,1002", new[] { 999, 1000, 0, 0 })]
    [InlineData("0,1001,0", new[] { 0, 0, 0 })]
    [InlineData("1001\n1002,1003", new[] { 0, 0, 0 })] // With newlines
    [InlineData("100\n1001,2000\n300", new[] { 100, 0, 0, 300 })]
    public void Parse_NumbersGreaterThan1000_AreConvertedToZero(string input, int[] expectedNumbers)
    {
        // Act
        var result = _parser.Parse(input);

        // Assert
        result.Should().NotBeNull();
        result.Numbers.Should().Equal(expectedNumbers);
    }

    [Theory]
    [InlineData("abc,1001,def", new[] { 0, 0, 0 })] // Invalid text and >1000
    [InlineData("999,notnumber,1001", new[] { 999, 0, 0 })]
    [InlineData("1001,xyz,2000", new[] { 0, 0, 0 })]
    public void Parse_InvalidNumbersAndGreaterThan1000_AllBecomeZero(string input, int[] expectedNumbers)
    {
        // Act
        var result = _parser.Parse(input);

        // Assert
        result.Should().NotBeNull();
        result.Numbers.Should().Equal(expectedNumbers);
    }

    [Theory]
    [InlineData("-1,1001,1002", new[] { -1, 0, 0 })] // Negative preserved, >1000 becomes 0
    [InlineData("1001,-1002,2000", new[] { 0, -1002, 0 })]
    [InlineData("-999,1000,1001", new[] { -999, 1000, 0 })]
    public void Parse_NegativeNumbersGreaterThan1000_NegativePreserved(string input, int[] expectedNumbers)
    {
        // Act
        var result = _parser.Parse(input);

        // Assert
        result.Should().NotBeNull();
        result.Numbers.Should().Equal(expectedNumbers);
    }

    [Fact]
    public void Parse_BoundaryValues_HandlesCorrectly()
    {
        // Test boundary around 1000
        var parser = new StringCalculatorParser();

        // Act
        var result999 = parser.Parse("999");
        var result1000 = parser.Parse("1000");
        var result1001 = parser.Parse("1001");

        // Assert
        result999.Numbers.Should().Equal(999);
        result1000.Numbers.Should().Equal(1000);
        result1001.Numbers.Should().Equal(0); // 1001 > 1000, so becomes 0
    }

    [Fact]
    public void Parse_VeryLargeNumbers_AreConvertedToZero()
    {
        // Arrange
        var input = "999999,1000000,-999999";

        // Act
        var result = _parser.Parse(input);

        // Assert
        result.Should().NotBeNull();
        result.Numbers.Should().Equal(0, 0, -999999); // Both large numbers become 0, negative preserved
    }
}
