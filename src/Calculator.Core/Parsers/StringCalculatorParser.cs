using Calculator.Core.Constants;
using Calculator.Core.Models;
using System.Text.RegularExpressions;

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

        // Check for multiple delimiters format: //[delim1][delim2]...\n
        if (delimiterSpec.StartsWith(CalculatorConstant.CustomDelimiterStart))
        {
            // Extract all delimiters between brackets
            var delimiters = ExtractDelimitersFromBrackets(delimiterSpec);

            if (delimiters.Count > 0)
            {
                // Parse numbers using multiple delimiters
                var numbers = SplitWithMultipleDelimiters(numbersPart, delimiters)
                    .Select(ParseNumber)
                    .ToList();

                request.Numbers = numbers;
                return request;
            }
        }

        // Fall back to single delimiter parsing (Requirements 6 & 7)
        return ParseWithSingleDelimiter(delimiterSpec, numbersPart, request);
    }

    private List<string> ExtractDelimitersFromBrackets(string delimiterSpec)
    {
        var delimiters = new List<string>();

        int position = 0;
        while (position < delimiterSpec.Length)
        {
            // Find next opening bracket
            int startBracket = delimiterSpec.IndexOf(CalculatorConstant.CustomDelimiterStart, position);
            if (startBracket == -1) break;

            // Find corresponding closing bracket
            int endBracket = delimiterSpec.IndexOf(CalculatorConstant.CustomDelimiterEnd, startBracket + 1);
            if (endBracket == -1) break;

            // Extract delimiter between brackets
            string delimiter = delimiterSpec[(startBracket + CalculatorConstant.CustomDelimiterStart.Length)..endBracket];
            if (!string.IsNullOrEmpty(delimiter))
            {
                delimiters.Add(delimiter);
            }

            // Move position after this delimiter
            position = endBracket + CalculatorConstant.CustomDelimiterEnd.Length;
        }

        return delimiters;
    }

    private List<string> SplitWithMultipleDelimiters(string input, List<string> delimiters)
    {
        if (delimiters.Count == 0)
            return new List<string> { input };

        if (delimiters.Count == 1)
            return [.. input.Split([delimiters[0]], StringSplitOptions.None)
                       .Select(s => s.Trim())
                       .Where(s => !string.IsNullOrEmpty(s))];

        // For multiple delimiters, we need to handle them carefully
        // Sort delimiters by length (longest first) to handle overlapping delimiters correctly
        var sortedDelimiters = delimiters.OrderByDescending(d => d.Length).ToList();

        // Build a regex pattern that matches any of the delimiters
        // We need to escape special regex characters in delimiters
        var escapedDelimiters = sortedDelimiters.Select(Regex.Escape);
        var pattern = "(" + string.Join("|", escapedDelimiters) + ")";

        // Split by all delimiters at once
        var parts = Regex.Split(input, pattern)
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrEmpty(s) && !delimiters.Contains(s))
            .ToList();

        return parts;
    }

    private CalculationRequest ParseWithSingleDelimiter(string delimiterSpec, string numbersPart, CalculationRequest request)
    {
        // Check if it's a bracket format (Requirement 7)
        if (delimiterSpec.Length >= 2 &&
            delimiterSpec.StartsWith(CalculatorConstant.CustomDelimiterStart) &&
            delimiterSpec.EndsWith(CalculatorConstant.CustomDelimiterEnd))
        {
            // Requirement 7: Delimiter of any length in brackets
            string customDelimiter = delimiterSpec[
                CalculatorConstant.CustomDelimiterStart.Length..^CalculatorConstant.CustomDelimiterEnd.Length];

            if (string.IsNullOrEmpty(customDelimiter))
            {
                // Empty delimiter - fall back to default
                var defaultRequest = new CalculationRequest { Input = numbersPart };
                return ParseWithDefaultDelimiters(numbersPart, defaultRequest);
            }

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
            var defaultRequest = new CalculationRequest { Input = numbersPart };
            return ParseWithDefaultDelimiters(numbersPart, defaultRequest);
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
            // Requirement 5: Numbers greater than MaxNumber are invalid (treated as 0)
            return number > CalculatorConstant.MaxNumberAllowed ? 0 : number;
        }

        return 0; // Invalid numbers are converted to 0
    }
}
