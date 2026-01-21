using Calculator.Core.Exceptions;

namespace Calculator.Core.Validators;

public class StringCalculatorValidator : IInputValidator
{
    public void Validate(List<int> numbers)
    {
        var negativeNumbers = numbers.Where(n => n < 0).ToList();

        if (negativeNumbers.Count != 0)
        {
            throw new NegativeNumberException(negativeNumbers);
        }
    }
}
