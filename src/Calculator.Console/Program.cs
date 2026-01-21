using Calculator.Console;
using Calculator.Core.Services;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.ConfigureServices();

var serviceProvider = services.BuildServiceProvider();

var calculator = serviceProvider.GetRequiredService<ICalculatorService>();

Console.WriteLine("String Calculator");
Console.WriteLine("Enter numbers separated by commas:");
Console.WriteLine("Enter 'exit' to quit.");

while (true)
{
    Console.Write("\nInput: ");
    var input = Console.ReadLine();

    if (input?.ToLower() == "exit")
        break;

    var result = calculator.Add(input);

    if (result.Success)
    {
        Console.WriteLine($"Result: {result.Result}");
    }
    else
    {
        Console.WriteLine($"Error: {result.ErrorMessage}");
    }
}
