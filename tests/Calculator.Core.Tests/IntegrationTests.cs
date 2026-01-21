using AwesomeAssertions;
using Calculator.Core.Parsers;
using Calculator.Core.Services;
using Calculator.Core.Validators;

namespace Calculator.Core.Tests;

public class IntegrationTests
{
    private readonly CalculatorService _calculator;

    public IntegrationTests()
    {
        var parser = new StringCalculatorParser();
        var validator = new StringCalculatorValidator();
        _calculator = new CalculatorService(parser, validator);
    }

    [Theory]
    [InlineData("", 0)]
    [InlineData("1", 1)]
    [InlineData("1,2", 3)]
    [InlineData("20", 20)]
    [InlineData("1,5000", 5001)]
    [InlineData("4,-3", 1)]
    [InlineData("5,tytyt", 5)]
    [InlineData("1,2,3,4,5,6,7,8,9,10,11,12", 78)]
    [InlineData("1\n2,3", 6)]
    [InlineData("10\n20,30", 60)]
    [InlineData("1\n2\n3\n4\n5", 15)]
    public void Add_WithRealDependencies_ReturnsCorrectResult(string input, int expected)
    {
        // Act
        var result = _calculator.Add(input);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Result.Should().Be(expected);
    }
}
