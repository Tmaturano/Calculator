using Calculator.Core.Constants;
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

        // Check for custom delimiter format
        if (input.StartsWith(CalculatorConstant.CustomDelimiterPrefix))
        {
            return ParseWithCustomDelimiter(input, request);
        }

        // Default parsing with comma and newline
        return ParseWithDefaultDelimiters(input, request);
    }

    private CalculationRequest ParseWithCustomDelimiter(string input, CalculationRequest request)
    {
        // Find the position of the first newline after the custom delimiter prefix
        int newLineIndex = input.IndexOf('\n');

        if (newLineIndex == -1)
        {
            // No newline found, treat as invalid format and parse with defaults
            return ParseWithDefaultDelimiters(input, request);
        }

        // Extract the delimiter specification (everything between // and \n)
        string delimiterSpec = input[CalculatorConstant.CustomDelimiterPrefix.Length..newLineIndex];

        // Extract the numbers part (everything after \n)
        string numbersPart = input[(newLineIndex + 1)..];
                
        char customDelimiter = delimiterSpec[0];

        // Parse numbers using custom delimiter
        var numbers = numbersPart.Split([customDelimiter], StringSplitOptions.RemoveEmptyEntries)
            .Select(ParseNumber)
            .ToList();

        request.Numbers = numbers;
        return request;
    }

    private CalculationRequest ParseWithDefaultDelimiters(string input, CalculationRequest request)
    {
        // Split by both comma and newline delimiters
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

        if (int.TryParse(numberString, out int number))
        {
            return number > CalculatorConstant.MaxNumberAllowed ? 0 : number;
        }

        return 0;
    }
}
