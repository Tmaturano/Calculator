namespace Calculator.Core.Models;

public class CalculationResult
{
    public int Result { get; set; }
    public string? Formula { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}