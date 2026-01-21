using Calculator.Core.Parsers;
using Calculator.Core.Services;
using Calculator.Core.Validators;
using Microsoft.Extensions.DependencyInjection;

namespace Calculator.Console;

public static class DependencyInjection
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services)
    {
        // Register core services
        services.AddSingleton<IInputParser, StringCalculatorParser>();
        services.AddSingleton<IInputValidator, StringCalculatorValidator>();
        services.AddSingleton<ICalculatorService, CalculatorService>();

        return services;
    }
}
