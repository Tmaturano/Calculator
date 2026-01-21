using Calculator.Core.Models;

namespace Calculator.Core.Parsers;

public interface IInputParser
{
    CalculationRequest Parse(string input);
}
