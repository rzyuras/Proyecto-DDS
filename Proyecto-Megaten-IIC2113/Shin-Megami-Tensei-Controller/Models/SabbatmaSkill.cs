namespace Shin_Megami_Tensei.Models;
using Shin_Megami_Tensei.Services;

public class SabbatmaSkill : SpecialSkill
{
    public override ActionResult Execute(Unit user, Unit target, Team userTeam, Team targetTeam, 
        TurnManager turnManager, DamageCalculator damageCalculator)
    {
        var result = new ActionResult { Success = true };
        
        if (!(target is Monster monster))
        {
            result.Messages.Add("Solo puede invocar monstruos");
            result.Success = false;
            return result;
        }
        
        if (!monster.IsAlive)
        {
            result.Messages.Add("No puede invocar monstruos muertos con Sabbatma");
            result.Success = false;
            return result;
        }
        
        result.Messages.Add($"{monster.Name} ha sido invocado");
        result.SpecialAction = "Summon";
        result.TargetUnit = monster;
        result.TurnConsumptionResult = turnManager.ConsumeNonOffensiveSkill();
        userTeam.SkillUsageCount++;
        
        return result;
    }
}