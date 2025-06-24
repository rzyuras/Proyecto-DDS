namespace Shin_Megami_Tensei.Services;
using Shin_Megami_Tensei.Models;
using Shin_Megami_Tensei.Common;

public class TurnManager
{
    public TurnInfo CurrentTurns { get; private set; } = new TurnInfo();
        
    public void ResetTurns(int aliveUnitsCount)
    {
        CurrentTurns.FullTurns = aliveUnitsCount;
        CurrentTurns.BlinkingTurns = 0;
    }
    
    public TurnConsumptionResult ConsumeTurnBasedOnAffinity(string affinityType)
    {
        switch(affinityType)
        {
            case GameConstants.RepelAffinity:
            case GameConstants.DrainAffinity:
                int currentFullTurns = CurrentTurns.FullTurns;
                int currentBlinkingTurns = CurrentTurns.BlinkingTurns;
    
                CurrentTurns.FullTurns = 0;
                CurrentTurns.BlinkingTurns = 0;
    
                return new TurnConsumptionResult 
                { 
                    FullTurnsConsumed = currentFullTurns,
                    BlinkingTurnsConsumed = currentBlinkingTurns,
                    BlinkingTurnsGained = 0
                };
            
            case GameConstants.NullAffinity:
                if (CurrentTurns.BlinkingTurns >= 2)
                {
                    CurrentTurns.BlinkingTurns -= 2;
                    return new TurnConsumptionResult 
                    { 
                        FullTurnsConsumed = 0, 
                        BlinkingTurnsConsumed = 2,
                        BlinkingTurnsGained = 0
                    };
                }
                else if (CurrentTurns.BlinkingTurns == 1)
                {
                    CurrentTurns.BlinkingTurns = 0;
                    if (CurrentTurns.FullTurns > 0)
                    {
                        CurrentTurns.FullTurns--;
                        return new TurnConsumptionResult
                        {
                            FullTurnsConsumed = 1,
                            BlinkingTurnsConsumed = 1,
                            BlinkingTurnsGained = 0
                        };
                    }
                    else
                    {
                        return new TurnConsumptionResult
                        {
                            FullTurnsConsumed = 0,
                            BlinkingTurnsConsumed = 1,
                            BlinkingTurnsGained = 0
                        };
                    }
                }
                else
                {
                    int fullTurnsToConsume = Math.Min(2, CurrentTurns.FullTurns);
                    CurrentTurns.FullTurns -= fullTurnsToConsume;
                    
                    return new TurnConsumptionResult
                    {
                        FullTurnsConsumed = fullTurnsToConsume,
                        BlinkingTurnsConsumed = 0,
                        BlinkingTurnsGained = 0
                    };
                }
            
            case GameConstants.WeakAffinity:
                if (CurrentTurns.FullTurns > 0)
                {
                    CurrentTurns.FullTurns--;
                    CurrentTurns.BlinkingTurns++;
                    
                    return new TurnConsumptionResult 
                    { 
                        FullTurnsConsumed = 1, 
                        BlinkingTurnsConsumed = 0,
                        BlinkingTurnsGained = 1
                    };
                }
                else if (CurrentTurns.BlinkingTurns > 0)
                {
                    CurrentTurns.BlinkingTurns--;
                    
                    return new TurnConsumptionResult 
                    { 
                        FullTurnsConsumed = 0, 
                        BlinkingTurnsConsumed = 1,
                        BlinkingTurnsGained = 0
                    };
                }
                break;
            
            case GameConstants.MissAffinity:
                if (CurrentTurns.BlinkingTurns > 0)
                {
                    CurrentTurns.BlinkingTurns--;
                    return new TurnConsumptionResult 
                    { 
                        FullTurnsConsumed = 0, 
                        BlinkingTurnsConsumed = 1,
                        BlinkingTurnsGained = 0
                    };
                }
                else if (CurrentTurns.FullTurns > 0)
                {
                    CurrentTurns.FullTurns--;
                    return new TurnConsumptionResult 
                    { 
                        FullTurnsConsumed = 1, 
                        BlinkingTurnsConsumed = 0,
                        BlinkingTurnsGained = 0
                    };
                }
                break;
                
            case GameConstants.NeutralAffinity:
            case GameConstants.ResistAffinity:
            default:
                if (CurrentTurns.BlinkingTurns > 0)
                {
                    CurrentTurns.BlinkingTurns--;
                    
                    return new TurnConsumptionResult 
                    { 
                        FullTurnsConsumed = 0, 
                        BlinkingTurnsConsumed = 1,
                        BlinkingTurnsGained = 0
                    };
                }
                else if (CurrentTurns.FullTurns > 0)
                {
                    CurrentTurns.FullTurns--;
                    
                    return new TurnConsumptionResult 
                    { 
                        FullTurnsConsumed = 1, 
                        BlinkingTurnsConsumed = 0,
                        BlinkingTurnsGained = 0
                    };
                }
                break;
        }
        
        return new TurnConsumptionResult();
    }
    
    public TurnConsumptionResult ConsumeInvokeOrPassTurn()
    {
        if (CurrentTurns.BlinkingTurns > 0)
        {
            CurrentTurns.BlinkingTurns--;
            
            return new TurnConsumptionResult 
            { 
                FullTurnsConsumed = 0, 
                BlinkingTurnsConsumed = 1,
                BlinkingTurnsGained = 0
            };
        }
        else if (CurrentTurns.FullTurns > 0)
        {
            CurrentTurns.FullTurns--;
            CurrentTurns.BlinkingTurns++;
            
            return new TurnConsumptionResult 
            { 
                FullTurnsConsumed = 1, 
                BlinkingTurnsConsumed = 0,
                BlinkingTurnsGained = 1
            };
        }
        
        return new TurnConsumptionResult();
    }
    
    public TurnConsumptionResult ConsumeNonOffensiveSkill()
    {
        if (CurrentTurns.BlinkingTurns > 0)
        {
            CurrentTurns.BlinkingTurns--;
            
            return new TurnConsumptionResult 
            { 
                FullTurnsConsumed = 0, 
                BlinkingTurnsConsumed = 1,
                BlinkingTurnsGained = 0
            };
        }
        else if (CurrentTurns.FullTurns > 0)
        {
            CurrentTurns.FullTurns--;
            
            return new TurnConsumptionResult 
            { 
                FullTurnsConsumed = 1, 
                BlinkingTurnsConsumed = 0,
                BlinkingTurnsGained = 0
            };
        }
        
        return new TurnConsumptionResult();
    }
        
    public int GetRemainingTurns()
    {
        return CurrentTurns.FullTurns + CurrentTurns.BlinkingTurns;
    }
}

public class TurnConsumptionResult
{
    public int FullTurnsConsumed { get; set; }
    public int BlinkingTurnsConsumed { get; set; }
    public int BlinkingTurnsGained { get; set; }
}