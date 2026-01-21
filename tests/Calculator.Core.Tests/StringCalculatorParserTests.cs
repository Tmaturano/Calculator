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
    public void Parse_MoreThanTwoNumbers_ThrowsArgumentException()
    {
        // Arrange
        var input = "1,2,3,4";

        // Act
        Action act = () => _parser.Parse(input);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Maximum of 2 numbers are allowed");
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
}
