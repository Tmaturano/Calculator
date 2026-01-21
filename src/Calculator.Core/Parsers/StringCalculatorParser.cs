using Calculator.Core.Models;

namespace Calculator.Core.Parsers;

public class StringCalculatorParser : IInputParser
{
    public CalculationRequest Parse(string input)
    {
        var request = new CalculationRequest { Input = input };

        if (string.IsNullOrEmpty(input))
        {
            request.Numbers.Add(0);
            return request;
        }

        var numbers = input.Split(',')
            .Select(ParseNumber)
            .ToList();

        // Requirement 1: Support max 2 numbers
        if (numbers.Count > 2)
        {
            throw new ArgumentException("Maximum of 2 numbers are allowed");
        }

        request.Numbers = numbers;
        return request;
    }

    private int ParseNumber(string numberString)
    {
        if (string.IsNullOrEmpty(numberString))
            return 0;

        return int.TryParse(numberString, out int number) ? number : 0;
    }
}
