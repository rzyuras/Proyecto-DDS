namespace Shin_Megami_Tensei.Models;

public class TurnInfo
{
    public int FullTurns { get; set; }
    public int BlinkingTurns { get; set; }
        
    public int RemainingTurns => FullTurns + BlinkingTurns;
}