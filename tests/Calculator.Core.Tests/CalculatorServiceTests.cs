using AwesomeAssertions;
using Calculator.Core.Exceptions;
using Calculator.Core.Models;
using Calculator.Core.Parsers;
using Calculator.Core.Services;
using Calculator.Core.Validators;
using NSubstitute;

namespace Calculator.Core.Tests;

public class CalculatorServiceTests
{
    private readonly IInputParser _parser;
    private readonly IInputValidator _validator;
    private readonly CalculatorService _calculator;

    public CalculatorServiceTests()
    {
        _parser = Substitute.For<IInputParser>();
        _validator = Substitute.For<IInputValidator>();        
        _calculator = new CalculatorService(_parser, _validator);
    }

    [Theory]
    [InlineData("", 0)]
    [InlineData("1", 1)]
    [InlineData("1,2", 3)]
    [InlineData("20", 20)]
    [InlineData("1,5000", 5001)]
    [InlineData("4,-3", 1)]
    public void Add_ValidInput_ReturnsCorrectSum(string input, int expected)
    {
        // Arrange
        var numbers = ParseNumbersFromString(input);
        var request = new CalculationRequest { Input = input, Numbers = numbers };

        _parser.Parse(input).Returns(request);
        _validator.When(v => v.Validate(numbers)).Do(x => { }); // No-op

        // Act
        var result = _calculator.Add(input);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Result.Should().Be(expected);
        result.ErrorMessage.Should().BeNull();
    }

    [Theory]
    [InlineData("5,tytyt", 5)]
    [InlineData("a,10", 10)]
    [InlineData("1,,3", 4)]
    public void Add_WithInvalidNumbers_ConvertsToZero(string input, int expected)
    {
        // Arrange
        var numbers = ParseNumbersFromString(input);
        var request = new CalculationRequest { Input = input, Numbers = numbers };

        _parser.Parse(input).Returns(request);
        _validator.When(v => v.Validate(numbers)).Do(x => { });

        // Act
        var result = _calculator.Add(input);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Result.Should().Be(expected);
    }

    [Fact]
    public void Add_WhenParserThrowsException_ReturnsFailureResult()
    {
        // Arrange
        var input = "invalid";
        var exceptionMessage = "Parsing failed";

        _parser.Parse(input)
            .Returns(x => throw new ArgumentException(exceptionMessage));

        // Act
        var result = _calculator.Add(input);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be(exceptionMessage);
        result.Result.Should().Be(0);
    }

    [Fact]
    public void Add_WhenValidatorThrowsNegativeNumberException_ReturnsFailureResult()
    {
        // Arrange
        var input = "1,-2,3";
        var numbers = new List<int> { 1, -2, 3 };
        var request = new CalculationRequest { Input = input, Numbers = numbers };
        var negativeNumbers = new List<int> { -2 };
        var exception = new NegativeNumberException(negativeNumbers);

        _parser.Parse(input).Returns(request);
        _validator.When(v => v.Validate(numbers))
            .Do(x => throw exception);

        // Act
        var result = _calculator.Add(input);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Negative numbers are not allowed");
        result.ErrorMessage.Should().Contain("-2");
    }

    [Fact]
    public void Add_CallsParserWithCorrectInput()
    {
        // Arrange
        var input = "1,2,3";
        var request = new CalculationRequest
        {
            Input = input,
            Numbers = [1, 2, 3]
        };

        _parser.Parse(input).Returns(request);

        // Act
        _calculator.Add(input);

        // Assert
        _parser.Received(1).Parse(input);
    }

    [Fact]
    public void Add_CallsValidatorWithParsedNumbers()
    {
        // Arrange
        var input = "1,2,3";
        var numbers = new List<int> { 1, 2, 3 };
        var request = new CalculationRequest
        {
            Input = input,
            Numbers = numbers
        };

        _parser.Parse(input).Returns(request);

        // Act
        _calculator.Add(input);

        // Assert
        _validator.Received(1).Validate(numbers);
    }

    [Theory]
    [InlineData("1,2,3", 6)]
    [InlineData("10,20,30,40", 100)]
    [InlineData("0,0,0,0,0", 0)]
    public void Add_MultipleNumbers_CalculatesCorrectSum(string input, int expected)
    {
        // Arrange
        var numbers = ParseNumbersFromString(input);
        var request = new CalculationRequest { Input = input, Numbers = numbers };

        _parser.Parse(input).Returns(request);
        _validator.When(v => v.Validate(numbers)).Do(x => { });

        // Act
        var result = _calculator.Add(input);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Result.Should().Be(expected);
    }

    [Theory]
    [InlineData("1\n2,3", 6)]
    [InlineData("1\n2\n3", 6)]
    [InlineData("10\n20,30", 60)]
    [InlineData("1\n2,3\n4,5\n6", 21)]
    [InlineData("100\n200\n300", 600)]
    [InlineData("\n1,2", 3)] // Leading newline
    [InlineData("1,2\n", 3)] // Trailing newline
    [InlineData("1\n\n2", 3)] // Multiple newlines
    public void Add_WithNewlineDelimiter_CalculatesCorrectSum(string input, int expected)
    {
        // Arrange
        var numbers = ParseNumbersWithNewlines(input);
        var request = new CalculationRequest { Input = input, Numbers = numbers };

        _parser.Parse(input).Returns(request);
        _validator.When(v => v.Validate(numbers)).Do(x => { });

        // Act
        var result = _calculator.Add(input);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Result.Should().Be(expected);
    }

    [Fact]
    public void Add_WithMixedDelimitersAndInvalidNumbers_HandlesCorrectly()
    {
        // Arrange
        var input = "1\nabc,3\ndef\n5";
        var numbers = new List<int> { 1, 0, 3, 0, 5 };
        var request = new CalculationRequest { Input = input, Numbers = numbers };

        _parser.Parse(input).Returns(request);
        _validator.When(v => v.Validate(numbers)).Do(x => { });

        // Act
        var result = _calculator.Add(input);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Result.Should().Be(9); // 1 + 0 + 3 + 0 + 5
    }

    [Fact]
    public void Add_WithOnlyNewlines_ReturnsZero()
    {
        // Arrange
        var input = "\n\n\n";
        var numbers = new List<int>();
        var request = new CalculationRequest { Input = input, Numbers = numbers };

        _parser.Parse(input).Returns(request);
        _validator.When(v => v.Validate(numbers)).Do(x => { });

        // Act
        var result = _calculator.Add(input);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Result.Should().Be(0);
    }


    // Helper method to parse numbers from string for test setup
    private List<int> ParseNumbersFromString(string input)
    {
        if (string.IsNullOrEmpty(input))
            return [0];

        return [.. input.Split([',', '\n'], StringSplitOptions.RemoveEmptyEntries).Select(s => int.TryParse(s, out int n) ? n : 0)];
    }

    // New helper method specifically for newline tests
    private List<int> ParseNumbersWithNewlines(string input)
    {
        if (string.IsNullOrEmpty(input))
            return [0];

        return [.. input.Split([',', '\n'], StringSplitOptions.RemoveEmptyEntries).Select(s => int.TryParse(s, out int n) ? n : 0)];
    }
}
