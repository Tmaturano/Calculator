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
    [InlineData("5,tytyt", 5)]
    [InlineData("1,2,3,4,5,6,7,8,9,10,11,12", 78)]
    [InlineData("1\n2,3", 6)]
    [InlineData("2,1001,6", 8)]
    [InlineData("//#\n2#5", 7)]
    [InlineData("//,\n2,ff,100", 102)]
    [InlineData("//[***]\n11***22***33", 66)]
    [InlineData("//[*][!!][r9r]\n11r9r22*hh*33!!44", 110)]
    [InlineData("//[*][%]\n1*2%3", 6)]
    [InlineData("//[**][%%]\n1**2%%3**4", 10)]
    [InlineData("//[sep][,]\n1sep2,3sep4", 10)]
    [InlineData("//[+][-][*]\n1+2-3*4", 10)]
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
    [InlineData("2,1001,-6", new[] { -6 })] // 1001 becomes 0, -6 is negative
    [InlineData("-1,1001,1002", new[] { -1 })] // >1000 numbers become 0
    [InlineData("1001,-1002,2000", new[] { -1002 })]
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

    [Fact]
    public void Add_BoundaryCondition1000_WorksCorrectly()
    {
        // Test numbers around the 1000 boundary
        var calculator = new CalculatorService(
            new StringCalculatorParser(),
            new StringCalculatorValidator());

        // Act & Assert
        calculator.Add("999").Result.Should().Be(999);
        calculator.Add("1000").Result.Should().Be(1000);
        calculator.Add("1001").Result.Should().Be(0);
        calculator.Add("999,1000,1001").Result.Should().Be(1999); // 999 + 1000 + 0
    }
}
