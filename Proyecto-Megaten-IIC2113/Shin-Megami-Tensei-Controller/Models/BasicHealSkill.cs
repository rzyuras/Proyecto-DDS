namespace Shin_Megami_Tensei.Models;
using Shin_Megami_Tensei.Services;

public class BasicHealSkill : HealSkill
{
    public override ActionResult Execute(Unit user, Unit target, Team userTeam, Team targetTeam, 
        TurnManager turnManager, DamageCalculator damageCalculator)
    {
        var result = new ActionResult { Success = true };
        
        int healAmount = (int)Math.Floor(target.MaxHP * (Power / 100.0));
        target.CurrentHP = Math.Min(target.MaxHP, target.CurrentHP + healAmount);
        
        result.Messages.Add($"{user.Name} cura a {target.Name}");
        result.Messages.Add($"{target.Name} recibe {healAmount} de HP");
        result.Messages.Add($"{target.Name} termina con HP:{target.CurrentHP}/{target.MaxHP}");
        
        result.TurnConsumptionResult = turnManager.ConsumeNonOffensiveSkill();
        userTeam.SkillUsageCount++;
        
        return result;
    }
}