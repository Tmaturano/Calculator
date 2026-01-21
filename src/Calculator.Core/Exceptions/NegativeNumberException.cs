namespace Calculator.Core.Exceptions;

public class NegativeNumberException : Exception
{
    public List<int> NegativeNumbers { get; }

    public NegativeNumberException(List<int> negativeNumbers)
        : base($"Negative numbers are not allowed: {string.Join(", ", negativeNumbers)}")
    {
        NegativeNumbers = negativeNumbers;
    }
}
