namespace Shin_Megami_Tensei.Models;
using Shin_Megami_Tensei.Services;

public class InvitationSkill : HealSkill
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
        
        result.Messages.Add($"{monster.Name} ha sido invocado");

        if (!monster.IsAlive)
        {
            monster.CurrentHP = monster.MaxHP;
            result.Messages.Add($"{user.Name} revive a {monster.Name}");
            result.Messages.Add($"{monster.Name} recibe {monster.MaxHP} de HP");
            result.Messages.Add($"{monster.Name} termina con HP:{monster.CurrentHP}/{monster.MaxHP}");
        }
        
        result.SpecialAction = "Summon";
        result.TargetUnit = monster;
        result.TurnConsumptionResult = turnManager.ConsumeNonOffensiveSkill();
        userTeam.SkillUsageCount++;
        
        return result;
    }
}