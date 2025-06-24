namespace Shin_Megami_Tensei.Models;
using Shin_Megami_Tensei.Common;

public abstract class Unit
{
    public string Name { get; set; }
    public int MaxHP { get; set; }
    public int CurrentHP { get; set; }
    public int MaxMP { get; set; }
    public int CurrentMP { get; set; }
    public int Str { get; set; }
    public int Skl { get; set; }
    public int Mag { get; set; }
    public int Spd { get; set; }
    public int Lck { get; set; }
    public Dictionary<string, string> Affinity { get; set; } = new Dictionary<string, string>();
        
    public bool IsAlive => CurrentHP > 0;
        
    public abstract List<string> GetAvailableActions();
        
    public int CalculatePhysicalDamage()
    {
        return Convert.ToInt32(Math.Floor(Str * GameConstants.AttackModifier * GameConstants.DamageMultiplier));
    }
        
    public int CalculateGunDamage()
    {
        return Convert.ToInt32(Math.Floor(Skl * GameConstants.ShootModifier * GameConstants.DamageMultiplier));
    }
        
    public void TakeDamage(int damage)
    {
        CurrentHP = Math.Max(0, CurrentHP - damage);
    }
    
    public int SkillUsageCount { get; set; } = 0;
    
    public string GetElementAttackVerb(string elementType)
    {
        switch (elementType)
        {
            case GameConstants.PhysElement:
                return "ataca a";
            case GameConstants.GunElement:
                return "dispara a";
            case GameConstants.FireElement:
                return "lanza fuego a";
            case GameConstants.IceElement:
                return "lanza hielo a";
            case GameConstants.ElecElement:
                return "lanza electricidad a";
            case GameConstants.ForceElement:
                return "lanza viento a";
            case GameConstants.LightElement:
                return "ataca con luz a";
            case GameConstants.DarkElement:
                return "ataca con oscuridad a";
            default:
                return "ataca a";
        }
    }
}