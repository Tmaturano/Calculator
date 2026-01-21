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
    [InlineData("1,5000", 1)]
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

    [Theory]
    [InlineData("1,-2,3", new[] { -2 })]
    [InlineData("-1,-2,-3", new[] { -1, -2, -3 })]
    [InlineData("10,-5,-3,2", new[] { -5, -3 })]
    [InlineData("-100", new[] { -100 })]
    [InlineData("0,-1,0", new[] { -1 })]
    public void Add_WithNegativeNumbers_ReturnsFailureWithMessage(string input, int[] expectedNegatives)
    {
        // Arrange
        var numbers = ParseNumbersFromString(input);
        var request = new CalculationRequest { Input = input, Numbers = numbers };

        _parser.Parse(input).Returns(request);

        // Setup validator to throw NegativeNumberException
        _validator.When(v => v.Validate(numbers))
            .Do(x => throw new NegativeNumberException(expectedNegatives.ToList()));

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
    public void Add_WithMultipleNegativeNumbers_IncludesAllInErrorMessage()
    {
        // Arrange
        var input = "1,-2,3,-4,5,-6";
        var negativeNumbers = new List<int> { -2, -4, -6 };
        var numbers = ParseNumbersFromString(input);
        var request = new CalculationRequest { Input = input, Numbers = numbers };

        _parser.Parse(input).Returns(request);
        _validator.When(v => v.Validate(numbers))
            .Do(x => throw new NegativeNumberException(negativeNumbers));

        // Act
        var result = _calculator.Add(input);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Negative numbers are not allowed: -2, -4, -6");
    }

    [Theory]
    [InlineData("1,2,3")]
    [InlineData("0,0,0")]
    [InlineData("10,20,30")]
    [InlineData("100,200,300")]
    public void Add_WithNoNegativeNumbers_ReturnsSuccess(string input)
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
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void Add_WithMixedValidators_ValidatesCorrectly()
    {
        // Arrange
        var input = "1,2,-3";
        var numbers = new List<int> { 1, 2, -3 };
        var request = new CalculationRequest { Input = input, Numbers = numbers };

        _parser.Parse(input).Returns(request);

        // Validator should be called with the parsed numbers
        _validator.When(v => v.Validate(Arg.Any<List<int>>()))
            .Do(x =>
            {
                var nums = x.Arg<List<int>>();
                if (nums.Any(n => n < 0))
                    throw new NegativeNumberException(nums.Where(n => n < 0).ToList());
            });

        // Act
        var result = _calculator.Add(input);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        _validator.Received(1).Validate(numbers);
    }

    [Theory]
    [InlineData("2,1001,6", 8)]
    [InlineData("1000,1001", 1000)] // 1000 is valid, 1001 becomes 0
    [InlineData("1001,1002,1003", 0)] // All numbers > 1000
    [InlineData("500,1500,2000,300", 800)] // 500 + 0 + 0 + 300
    [InlineData("999,1000,1001,1002", 1999)] // 999 + 1000 + 0 + 0
    [InlineData("0,1001,0", 0)] // Only 1001 which becomes 0
    public void Add_NumbersGreaterThan1000_AreTreatedAsZero(string input, int expected)
    {
        // Arrange
        var numbers = ParseNumbersFromStringWithMax1000(input);
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
    [InlineData("2,1001,-6", new[] { -6 })] // 1001 becomes 0, -6 is negative
    [InlineData("-1,1001,1002", new[] { -1 })] // Negative takes precedence
    [InlineData("1001,-1002,2000", new[] { -1002 })] // Multiple >1000 and one negative
    public void Add_NumbersGreaterThan1000WithNegatives_ReturnsNegativeError(string input, int[] expectedNegatives)
    {
        // Arrange
        var numbers = ParseNumbersFromStringWithMax1000(input);
        var request = new CalculationRequest { Input = input, Numbers = numbers };

        _parser.Parse(input).Returns(request);
        _validator.When(v => v.Validate(numbers))
            .Do(x => throw new NegativeNumberException([.. expectedNegatives]));

        // Act
        var result = _calculator.Add(input);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Negative numbers are not allowed");
    }

    [Fact]
    public void Add_NumbersGreaterThan1000WithNewlines_HandlesCorrectly()
    {
        // Arrange
        var input = "100\n1001,2000\n300";
        var numbers = new List<int> { 100, 0, 0, 300 }; // 1001 and 2000 become 0
        var request = new CalculationRequest { Input = input, Numbers = numbers };

        _parser.Parse(input).Returns(request);
        _validator.When(v => v.Validate(numbers)).Do(x => { });

        // Act
        var result = _calculator.Add(input);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Result.Should().Be(400); // 100 + 0 + 0 + 300
    }

    [Theory]
    [InlineData("//#\n2#5", 7)] // Requirement 6 example
    [InlineData("//;\n1;2;3", 6)]
    [InlineData("//;\n1;2", 3)]
    [InlineData("//,\n2,ff,100", 102)] // Requirement 6 example
    [InlineData("//*\n4*5*6", 15)]
    [InlineData("// \n1 2 3", 6)] // Space as delimiter
    [InlineData("//-\n10-20-30", 60)]
    [InlineData("//.\n1.2.3.4.5", 15)]
    public void Add_WithCustomSingleCharDelimiter_ReturnsCorrectSum(string input, int expected)
    {
        // Arrange
        var numbers = ParseNumbersWithCustomDelimiter(input);
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
    public void Add_WithCustomDelimiterAndLargeNumbers_HandlesCorrectly()
    {
        // Arrange
        var input = "//#\n2#1001#6";
        var numbers = new List<int> { 2, 0, 6 }; // 1001 becomes 0
        var request = new CalculationRequest { Input = input, Numbers = numbers };

        _parser.Parse(input).Returns(request);
        _validator.When(v => v.Validate(numbers)).Do(x => { });

        // Act
        var result = _calculator.Add(input);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Result.Should().Be(8); // 2 + 0 + 6
    }

    [Fact]
    public void Add_WithCustomDelimiterAndNegativeNumbers_ReturnsError()
    {
        // Arrange
        var input = "//;\n1;-2;3;-4";
        var numbers = new List<int> { 1, -2, 3, -4 };
        var request = new CalculationRequest { Input = input, Numbers = numbers };

        _parser.Parse(input).Returns(request);
        _validator.When(v => v.Validate(numbers))
            .Do(x => throw new NegativeNumberException(new List<int> { -2, -4 }));

        // Act
        var result = _calculator.Add(input);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Negative numbers are not allowed: -2, -4");
    }

    [Theory]
    [InlineData("//#\n2#tytyt#100", 102)] // Invalid number becomes 0
    [InlineData("//;\na;b;c", 0)] // All invalid
    [InlineData("//,\n,2,,4,", 6)] // Empty entries
    public void Add_WithCustomDelimiterAndInvalidNumbers_HandlesCorrectly(string input, int expected)
    {
        // Arrange
        var numbers = ParseNumbersWithCustomDelimiter(input);
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

    // Helper method for custom delimiter parsing
    private List<int> ParseNumbersWithCustomDelimiter(string input)
    {
        if (string.IsNullOrEmpty(input))
            return [0];

        // Simple implementation for test setup
        if (input.StartsWith("//"))
        {
            int newLineIndex = input.IndexOf('\n');
            if (newLineIndex == -1)
                return [0];

            char delimiter = input[2]; // Character after "//"
            string numbersPart = input[(newLineIndex + 1)..];

            return [.. numbersPart.Split(delimiter, StringSplitOptions.RemoveEmptyEntries)
                .Select(s =>
                {
                    if (int.TryParse(s, out int n))
                        return n > 1000 ? 0 : n;
                    return 0;
                })];
        }

        return ParseNumbersFromString(input);
    }

    [Theory]
    [InlineData("//[***]\n11***22***33", 66)] // Requirement 7 example
    [InlineData("//[---]\n10---20---30", 60)]
    [InlineData("//[xyz]\n1xyz2xyz3xyz4", 10)]
    [InlineData("//[;]\n1;2;3", 6)] // Single character in brackets (still works)
    [InlineData("//[***]\n1***1001***2", 3)] // With >1000 number
    [InlineData("//[sep]\n10sep20sep30sep40", 100)]
    [InlineData("//[**]\n5**10**15", 30)]
    [InlineData("//[!!!!]\n1!!!!2!!!!3!!!!4!!!!5", 15)]
    public void Add_WithCustomDelimiterAnyLength_ReturnsCorrectSum(string input, int expected)
    {
        // Arrange
        var numbers = ParseNumbersWithCustomDelimiterAnyLength(input);
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
    [InlineData("//[*][!!][r9r]\n11r9r22*hh*33!!44", 110)]
    [InlineData("//[*][%]\n1*2%3", 6)]
    [InlineData("//[**][%%]\n1**2%%3**4", 10)]
    [InlineData("//[sep][,]\n1sep2,3sep4", 10)]
    [InlineData("//[+][-][*]\n1+2-3*4", 10)]
    [InlineData("//[abc][def][ghi]\n1abc2def3ghi4", 10)]
    [InlineData("//[!!][??]\n10!!20??30!!40", 100)]
    [InlineData("//[**][***][****]\n1**2***3****4", 10)]
    public void Add_WithMultipleCustomDelimiters_ReturnsCorrectSum(string input, int expected)
    {
        var parser = new StringCalculatorParser();
        var validator = new StringCalculatorValidator();
        var calculator = new CalculatorService(parser, validator);

        // Act
        var result = calculator.Add(input);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Result.Should().Be(expected);
    }

    #region Helper Methods   

    // Helper method for parsing with any length delimiter
    private List<int> ParseNumbersWithCustomDelimiterAnyLength(string input)
    {
        if (string.IsNullOrEmpty(input))
            return [0];

        if (input.StartsWith("//["))
        {
            int newLineIndex = input.IndexOf('\n');
            if (newLineIndex == -1)
                return [0];

            // Extract delimiter between [ and ]
            int startBracket = input.IndexOf('[');
            int endBracket = input.IndexOf(']');

            if (startBracket == -1 || endBracket == -1 || endBracket <= startBracket)
                return [0];

            string delimiter = input[(startBracket + 1)..endBracket];
            string numbersPart = input[(newLineIndex + 1)..];

            return [.. numbersPart.Split([delimiter], StringSplitOptions.RemoveEmptyEntries)
                .Select(s =>
                {
                    if (int.TryParse(s, out int n))
                        return n > 1000 ? 0 : n;
                    return 0;
                })];
        }

        return ParseNumbersWithCustomDelimiter(input);
    }

    // New helper method that respects the 1000 limit
    private List<int> ParseNumbersFromStringWithMax1000(string input)
    {
        if (string.IsNullOrEmpty(input))
            return [0];

        return [.. input.Split([',', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Select(s =>
            {
                if (int.TryParse(s, out int n))
                {
                    return n > 1000 ? 0 : n;
                }
                return 0;
            })];
    }


    // Helper method to parse numbers from string for test setup
    private List<int> ParseNumbersFromString(string input)
    {
        if (string.IsNullOrEmpty(input))
            return [0];

        return [.. input.Split([',', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Select(s =>
            {
                if (int.TryParse(s, out int n))
                    return n > 1000 ? 0 : n;
                return 0;
            })];
    }

    // New helper method specifically for newline tests
    private List<int> ParseNumbersWithNewlines(string input)
    {
        if (string.IsNullOrEmpty(input))
            return [0];

        return [.. input.Split([',', '\n'], StringSplitOptions.RemoveEmptyEntries).Select(s => int.TryParse(s, out int n) ? n : 0)];
    }

    #endregion
}
