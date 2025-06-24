namespace Shin_Megami_Tensei.Models;
using Shin_Megami_Tensei.Services;

public class ActionResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public List<string> Messages { get; set; } = new List<string>();
    public bool GameEnded { get; set; }
    public string Winner { get; set; }
    public int DamageDealt { get; set; }
    public bool Cancelled { get; set; }
    public TurnConsumptionResult TurnConsumptionResult { get; set; }
    public string SpecialAction { get; set; }
    public Unit TargetUnit { get; set; }
    public string AffinityType { get; set; }
}