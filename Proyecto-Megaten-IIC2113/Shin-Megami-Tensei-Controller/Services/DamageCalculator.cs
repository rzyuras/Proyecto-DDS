namespace Shin_Megami_Tensei.Services;
using Shin_Megami_Tensei.Models;
using Shin_Megami_Tensei.Common;

public class DamageCalculator
{
    public int CalculateAttackDamage(Unit attacker, Unit target)
    {
        double baseDamage = attacker.Str * GameConstants.AttackModifier * GameConstants.DamageMultiplier;
        return ApplyAffinityModifier(baseDamage, target, GameConstants.PhysElement);
    }
    
    public int CalculateShootDamage(Unit attacker, Unit target)
    {
        double baseDamage = attacker.Skl * GameConstants.ShootModifier * GameConstants.DamageMultiplier;
        return ApplyAffinityModifier(baseDamage, target, GameConstants.GunElement);
    }

    public int CalculateSkillDamage(Unit attacker, Unit target, string elementType, int skillPower)
    {
        double baseDamage;
        if (elementType == GameConstants.PhysElement)
        {
            baseDamage = Math.Sqrt(attacker.Str * skillPower);
        }
        else if (elementType == GameConstants.GunElement)
        {
            baseDamage = Math.Sqrt(attacker.Skl * skillPower);
        }
        else
        {
            baseDamage = Math.Sqrt(attacker.Mag * skillPower);
        }
    
        return ApplyAffinityModifier(baseDamage, target, elementType);
    }
    
    private int ApplyAffinityModifier(double baseDamage, Unit target, string elementType)
    {
        if (!target.Affinity.ContainsKey(elementType))
            return Convert.ToInt32(Math.Floor(baseDamage));
        
        string affinity = target.Affinity[elementType];
    
        switch(affinity)
        {
            case GameConstants.WeakAffinity:
                return Convert.ToInt32(Math.Floor(baseDamage * GameConstants.WeakDamageMultiplier));
            case GameConstants.ResistAffinity:
                return Convert.ToInt32(Math.Floor(baseDamage * GameConstants.ResistDamageMultiplier));
            case GameConstants.NullAffinity:
                return 0;
            case GameConstants.RepelAffinity:
                return -Convert.ToInt32(Math.Floor(baseDamage));
            case GameConstants.DrainAffinity:
                return Convert.ToInt32(Math.Floor(baseDamage) * GameConstants.DrainDamageMultiplier);
            case GameConstants.NeutralAffinity: 
            default:
                return Convert.ToInt32(Math.Floor(baseDamage));
        }
    }
    
    public AffinityResult GetAffinityResult(Unit target, string elementType)
    {
        if (!target.Affinity.ContainsKey(elementType))
            return new AffinityResult { Type = GameConstants.NeutralAffinity };
            
        string affinity = target.Affinity[elementType];
        
        return new AffinityResult { Type = affinity };
    }
}

public class AffinityResult
{
    public string Type { get; set; }
    public bool IsSuccessfulInstantKill { get; set; }
}