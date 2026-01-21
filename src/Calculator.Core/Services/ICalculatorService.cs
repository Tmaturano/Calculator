using Calculator.Core.Models;

namespace Calculator.Core.Services;

public interface ICalculatorService
{
    CalculationResult Add(string input);
}
