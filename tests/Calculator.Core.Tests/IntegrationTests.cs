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

    [Theory]
    [InlineData("1,-2,3", new[] { -2 })]
    [InlineData("-1", new[] { -1 })]
    [InlineData("-1,-2,-3", new[] { -1, -2, -3 })]
    [InlineData("10,-5,-3,2", new[] { -5, -3 })]
    [InlineData("1\n-2,3", new[] { -2 })]
    [InlineData("-1\n-2\n-3", new[] { -1, -2, -3 })]
    public void Add_WithNegativeNumbers_ReturnsFailure(string input, int[] expectedNegatives)
    {
        // Act
        var result = _calculator.Add(input);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Negative numbers are not allowed");

        foreach (var negative in expectedNegatives)
        {
            result.ErrorMessage.Should().Contain(negative.ToString());
        }
    }

    [Fact]
    public void Add_ExampleFromRequirements_ThrowsException()
    {
        // Arrange
        var input = "1,-2,3,-4";

        // Act
        var result = _calculator.Add(input);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("-2");
        result.ErrorMessage.Should().Contain("-4");
    }

    [Fact]
    public void Add_MixedPositiveAndNegativeWithNewlines_HandlesCorrectly()
    {
        // Arrange
        var input = "10\n-20,30\n-40";

        // Act
        var result = _calculator.Add(input);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("-20");
        result.ErrorMessage.Should().Contain("-40");
    }
}
