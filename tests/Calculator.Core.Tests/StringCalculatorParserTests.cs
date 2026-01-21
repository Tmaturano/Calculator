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
}
