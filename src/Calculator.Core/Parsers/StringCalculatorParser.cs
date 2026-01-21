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

        var delimiters = new[] { ',', '\n' };
        var numbers = input.Split(delimiters, StringSplitOptions.RemoveEmptyEntries)
            .Select(ParseNumber)
            .ToList();

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
