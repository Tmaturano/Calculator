using Calculator.Core.Exceptions;
using Calculator.Core.Models;
using Calculator.Core.Parsers;
using Calculator.Core.Validators;

namespace Calculator.Core.Services;

public class CalculatorService : ICalculatorService
{
    private readonly IInputParser _parser;
    private readonly IInputValidator _validator;

    public CalculatorService(IInputParser parser, IInputValidator validator)
    {
        _parser = parser;
        _validator = validator;
    }

    public CalculationResult Add(string input)
    {
        try
        {
            var request = _parser.Parse(input);
            _validator.Validate(request.Numbers);

            var sum = request.Numbers.Sum();

            return new CalculationResult
            {
                Result = sum,
                Success = true
            };
        }
        catch (Exception ex)
        {
            var errorMessage = ex switch
            {
                NegativeNumberException ne => ne.Message,
                _ => ex.Message
            };

            return new CalculationResult
            {
                Success = false,
                ErrorMessage = errorMessage
            };
        }
    }
}
