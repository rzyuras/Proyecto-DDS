namespace Shin_Megami_Tensei.Models;
using Shin_Megami_Tensei.Services;

public class SpecialSkill : Skill
{
    public override ActionResult Execute(Unit user, Unit target, Team userTeam, Team targetTeam, 
        TurnManager turnManager, DamageCalculator damageCalculator)
    {
        var result = new ActionResult { Success = true };
        result.TurnConsumptionResult = turnManager.ConsumeNonOffensiveSkill();
        userTeam.SkillUsageCount++;
        return result;
    }
}