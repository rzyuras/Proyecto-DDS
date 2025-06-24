namespace Shin_Megami_Tensei.Models;
using Shin_Megami_Tensei.Services;

public class RecarmSkill : HealSkill
{
    public override ActionResult Execute(Unit user, Unit target, Team userTeam, Team targetTeam, 
        TurnManager turnManager, DamageCalculator damageCalculator)
    {
        var result = new ActionResult { Success = true };
        
        if (target.IsAlive)
        {
            result.Messages.Add($"{target.Name} ya está vivo");
            result.Success = false;
            return result;
        }
        
        int healAmount = Name.Contains("Samarecarm") ? target.MaxHP : target.MaxHP / 2;
        target.CurrentHP = healAmount;
        
        result.Messages.Add($"{user.Name} revive a {target.Name}");
        result.Messages.Add($"{target.Name} recibe {healAmount} de HP");
        result.Messages.Add($"{target.Name} termina con HP:{target.CurrentHP}/{target.MaxHP}");
        
        result.TurnConsumptionResult = turnManager.ConsumeNonOffensiveSkill();
        userTeam.SkillUsageCount++;
        
        return result;
    }
}