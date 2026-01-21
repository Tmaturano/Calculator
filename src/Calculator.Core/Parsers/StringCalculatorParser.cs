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

        if (newLineIndex == -1 || newLineIndex <= CalculatorConstant.CustomDelimiterPrefix.Length)
        {
            // No newline found or delimiter specification is empty
            // Treat as invalid format and parse with defaults
            return ParseWithDefaultDelimiters(input, request);
        }

        // Extract the delimiter specification (everything between // and \n)
        string delimiterSpec = input[CalculatorConstant.CustomDelimiterPrefix.Length..newLineIndex];

        // Extract the numbers part (everything after \n)
        string numbersPart = input[(newLineIndex + 1)..];

        // Check if it's a single character delimiter (Requirement 6) or bracket format (Requirement 7)
        if (delimiterSpec.StartsWith(CalculatorConstant.CustomDelimiterStart) &&
            delimiterSpec.EndsWith(CalculatorConstant.CustomDelimiterEnd))
        {
            // Requirement 7: Delimiter of any length in brackets
            string customDelimiter = delimiterSpec[
                CalculatorConstant.CustomDelimiterStart.Length..^CalculatorConstant.CustomDelimiterEnd.Length];

            // Parse numbers using custom delimiter of any length
            var numbers = numbersPart.Split([customDelimiter], StringSplitOptions.RemoveEmptyEntries)
                .Select(ParseNumber)
                .ToList();

            request.Numbers = numbers;
            return request;
        }
        else if (delimiterSpec.Length == 1)
        {
            // Requirement 6: Single character delimiter
            char customDelimiter = delimiterSpec[0];

            // Parse numbers using custom delimiter
            var numbers = numbersPart.Split([customDelimiter], StringSplitOptions.RemoveEmptyEntries)
                .Select(ParseNumber)
                .ToList();

            request.Numbers = numbers;
            return request;
        }
        else
        {
            // Invalid delimiter format - fall back to default parsing
            return ParseWithDefaultDelimiters(input, request);
        }
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
