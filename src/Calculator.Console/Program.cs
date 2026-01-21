using Calculator.Console;
using Calculator.Core.Services;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.ConfigureServices();

var serviceProvider = services.BuildServiceProvider();

var calculator = serviceProvider.GetRequiredService<ICalculatorService>();

Console.WriteLine("String Calculator");
Console.WriteLine("Enter numbers separated by commas or newlines:");
Console.WriteLine("Examples:");
Console.WriteLine("  1,2,3                     -> Result: 6");
Console.WriteLine("  1\\n2,3                   -> Result: 6");
Console.WriteLine("  //#\\n2#5                 -> Result: 7");
Console.WriteLine("  //[***]\\n11***22***33     -> Result: 66");
Console.WriteLine("  //[*][!!][r9r]\\n11r9r22*hh*33!!44 -> Result: 110");
Console.WriteLine("Enter 'exit' to quit.");

while (true)
{
    Console.Write("\nInput: ");
    var input = Console.ReadLine();

    if (input?.ToLower() == "exit")
        break;

    // Convert literal "\n" to actual newline character
    if (input != null)
    {
        input = input.Replace("\\n", "\n", StringComparison.Ordinal);
    }

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
