namespace Calculator.Core.Models;

public class CalculationRequest
{
    public string Input { get; set; } = string.Empty;
    public List<int> Numbers { get; set; } = [];
    public string? Formula { get; set; }
}
