using AwesomeAssertions;
using Calculator.Core.Exceptions;
using Calculator.Core.Validators;

namespace Calculator.Core.Tests;

public class StringCalculatorValidatorTests
{
    private readonly StringCalculatorValidator _validator;

    public StringCalculatorValidatorTests()
    {
        _validator = new StringCalculatorValidator();
    }

    [Fact]
    public void Validate_EmptyList_DoesNotThrow()
    {
        // Arrange
        var numbers = new List<int>();

        // Act
        Action act = () => _validator.Validate(numbers);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_AllPositiveNumbers_DoesNotThrow()
    {
        // Arrange
        var numbers = new List<int> { 1, 2, 3, 4, 5 };

        // Act
        Action act = () => _validator.Validate(numbers);

        // Assert
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(new[] { -1 })]
    [InlineData(new[] { -1, -2 })]
    [InlineData(new[] { 1, -2, 3 })]
    [InlineData(new[] { -10, 20, -30 })]
    public void Validate_WithNegativeNumbers_ThrowsNegativeNumberException(int[] numbersArray)
    {
        // Arrange
        var numbers = numbersArray.ToList();
        var expectedNegatives = numbers.Where(n => n < 0).ToList();

        // Act
        Action act = () => _validator.Validate(numbers);

        // Assert
        act.Should().Throw<NegativeNumberException>()
            .WithMessage($"Negative numbers are not allowed: {string.Join(", ", expectedNegatives)}")
            .Which.NegativeNumbers.Should().Equal(expectedNegatives);
    }

    [Fact]
    public void Validate_SingleNegativeNumber_ThrowsWithCorrectMessage()
    {
        // Arrange
        var numbers = new List<int> { 1, 2, -3, 4 };

        // Act
        Action act = () => _validator.Validate(numbers);

        // Assert
        act.Should().Throw<NegativeNumberException>()
            .WithMessage("Negative numbers are not allowed: -3")
            .Which.NegativeNumbers.Should().Equal(-3);
    }

    [Fact]
    public void Validate_MultipleNegativeNumbers_ThrowsWithAllNumbers()
    {
        // Arrange
        var numbers = new List<int> { -1, 2, -3, 4, -5 };

        // Act
        Action act = () => _validator.Validate(numbers);

        // Assert
        act.Should().Throw<NegativeNumberException>()
            .WithMessage("Negative numbers are not allowed: -1, -3, -5")
            .Which.NegativeNumbers.Should().Equal(-1, -3, -5);
    }

    [Fact]
    public void Validate_AllNegativeNumbers_ThrowsWithAllNumbers()
    {
        // Arrange
        var numbers = new List<int> { -1, -2, -3, -4 };

        // Act
        Action act = () => _validator.Validate(numbers);

        // Assert
        act.Should().Throw<NegativeNumberException>()
            .WithMessage("Negative numbers are not allowed: -1, -2, -3, -4")
            .Which.NegativeNumbers.Should().Equal(-1, -2, -3, -4);
    }
}
