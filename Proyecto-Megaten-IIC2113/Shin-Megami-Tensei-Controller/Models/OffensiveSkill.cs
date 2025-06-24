namespace Shin_Megami_Tensei.Models;
using Shin_Megami_Tensei.Services;
using Shin_Megami_Tensei.Common;

public class OffensiveSkill : Skill
{
    public override ActionResult Execute(Unit user, Unit target, Team userTeam, Team targetTeam, TurnManager turnManager, DamageCalculator damageCalculator)
    {
        if (Type == GameConstants.LightElement || Type == GameConstants.DarkElement)
        {
            return ExecuteInstantKillSkill(user, target, userTeam, turnManager, damageCalculator);
        }
        
        return ExecuteRegularOffensiveSkill(user, target, userTeam, turnManager, damageCalculator);
    }

    private ActionResult ExecuteInstantKillSkill(Unit user, Unit target, Team userTeam, TurnManager turnManager, DamageCalculator damageCalculator)
    {
        List<string> messages = new List<string>();
        AffinityResult affinityResult = damageCalculator.GetAffinityResult(target, Type);
        
        string attackVerb = user.GetElementAttackVerb(Type);
        messages.Add($"{user.Name} {attackVerb} {target.Name}");
        
        bool instantKillSuccess = false;
        
        switch (affinityResult.Type)
        {
            case GameConstants.WeakAffinity:
                messages.Add($"{target.Name} es débil contra el ataque de {user.Name}");
                instantKillSuccess = true;
                break;
                
            case GameConstants.NeutralAffinity:
                instantKillSuccess = (user.Lck + Power) >= target.Lck;
                break;
                
            case GameConstants.ResistAffinity:
                messages.Add($"{target.Name} es resistente el ataque de {user.Name}");
                instantKillSuccess = (user.Lck + Power) >= (2 * target.Lck);
                break;
                
            case GameConstants.NullAffinity:
                messages.Add($"{target.Name} bloquea el ataque de {user.Name}");
                instantKillSuccess = false;
                break;
                
            case GameConstants.RepelAffinity:
                messages.Add($"{target.Name} devuelve el ataque a {user.Name}");
                user.CurrentHP = 0;
                messages.Add($"{user.Name} ha sido eliminado");
                messages.Add($"{user.Name} termina con HP:0/{user.MaxHP}");
                break;
                
            case GameConstants.DrainAffinity:
                instantKillSuccess = false;
                break;
        }
        
        if (instantKillSuccess && affinityResult.Type != GameConstants.RepelAffinity)
        {
            target.CurrentHP = 0;
            messages.Add($"{target.Name} ha sido eliminado");
            messages.Add($"{target.Name} termina con HP:0/{target.MaxHP}");
        }
        else if (!instantKillSuccess && affinityResult.Type != GameConstants.NullAffinity && affinityResult.Type != GameConstants.RepelAffinity)
        {
            messages.Add($"{user.Name} ha fallado el ataque");
            messages.Add($"{target.Name} termina con HP:{target.CurrentHP}/{target.MaxHP}");
        }
        
        userTeam.SkillUsageCount++;
        TurnConsumptionResult turnResult = turnManager.ConsumeTurnBasedOnAffinity(affinityResult.Type);
        
        return new ActionResult
        {
            Success = true,
            Messages = messages,
            TurnConsumptionResult = turnResult,
            AffinityType = affinityResult.Type
        };
    }

    private ActionResult ExecuteRegularOffensiveSkill(Unit user, Unit target, Team userTeam, TurnManager turnManager, DamageCalculator damageCalculator)
    {
        List<string> messages = new List<string>();
        int totalDamage = 0;
        
        int hitCount = GetHitCount(userTeam.SkillUsageCount);
        AffinityResult affinityResult = damageCalculator.GetAffinityResult(target, Type);
        
        if (affinityResult.Type == GameConstants.RepelAffinity || affinityResult.Type == GameConstants.DrainAffinity)
        {
            for (int i = 0; i < hitCount; i++)
            {
                int damage = damageCalculator.CalculateSkillDamage(user, target, Type, Power);
                string attackVerb = user.GetElementAttackVerb(Type);
                messages.Add($"{user.Name} {attackVerb} {target.Name}");
                
                if (affinityResult.Type == GameConstants.RepelAffinity)
                {
                    messages.Add($"{target.Name} devuelve {Math.Abs(damage)} daño a {user.Name}");
                    user.TakeDamage(Math.Abs(damage));
                }
                else if (affinityResult.Type == GameConstants.DrainAffinity)
                {
                    messages.Add($"{target.Name} absorbe {Math.Abs(damage)} daño");
                    target.CurrentHP = Math.Min(target.MaxHP, target.CurrentHP + Math.Abs(damage));
                }
            }
            
            if (affinityResult.Type == GameConstants.RepelAffinity)
            {
                messages.Add($"{user.Name} termina con HP:{user.CurrentHP}/{user.MaxHP}");
            }
            else if (affinityResult.Type == GameConstants.DrainAffinity)
            {
                messages.Add($"{target.Name} termina con HP:{target.CurrentHP}/{target.MaxHP}");
            }
        }
        else
        {
            for (int i = 0; i < hitCount; i++)
            {
                int damage = damageCalculator.CalculateSkillDamage(user, target, Type, Power);
                string attackVerb = user.GetElementAttackVerb(Type);
                messages.Add($"{user.Name} {attackVerb} {target.Name}");
                
                if (affinityResult.Type == GameConstants.WeakAffinity)
                {
                    messages.Add($"{target.Name} es débil contra el ataque de {user.Name}");
                }
                else if (affinityResult.Type == GameConstants.ResistAffinity)
                {
                    messages.Add($"{target.Name} es resistente el ataque de {user.Name}");
                }
                else if (affinityResult.Type == GameConstants.NullAffinity)
                {
                    messages.Add($"{target.Name} bloquea el ataque de {user.Name}");
                }
                
                if (affinityResult.Type != GameConstants.NullAffinity)
                {
                    target.TakeDamage(damage);
                    messages.Add($"{target.Name} recibe {damage} de daño");
                    totalDamage += damage;
                }
            }
            
            messages.Add($"{target.Name} termina con HP:{target.CurrentHP}/{target.MaxHP}");
        }
        
        userTeam.SkillUsageCount++;
        
        TurnConsumptionResult turnResult = turnManager.ConsumeTurnBasedOnAffinity(affinityResult.Type);
        
        return new ActionResult
        {
            Success = true,
            Messages = messages,
            DamageDealt = totalDamage,
            TurnConsumptionResult = turnResult,
            AffinityType = affinityResult.Type
        };
    }
}