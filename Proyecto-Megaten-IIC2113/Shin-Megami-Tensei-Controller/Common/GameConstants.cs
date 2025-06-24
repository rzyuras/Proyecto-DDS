namespace Shin_Megami_Tensei.Common;

public class GameConstants
{
    public const double DamageMultiplier = 0.0114;
    public const int AttackModifier = 54;
    public const int ShootModifier = 80;
    
    public const int MaxSkillsPerSamurai = 8;
    public const int MaxMonstersPerTeam = 7;
    public const int TotalMaxUnitsPerTeam = 8;

    public const int BoardPositionsPerTeam = 4;
    public const int InitialPositionForSamurai = 0;

    public const string SeparatorLine = "----------------------------------------";

    public const string BoardPositionFormat = "{0} - {1} HP:{2}/{3} MP:{4}/{5}";
    public const string EmptyPositionFormat = "{0} -";

    public const string NeutralAffinity = "-";
    public const string WeakAffinity = "Wk";
    public const string ResistAffinity = "Rs";
    public const string NullAffinity = "Nu";
    public const string RepelAffinity = "Rp";
    public const string DrainAffinity = "Dr";
    public const string MissAffinity = "Miss";

    public const string PhysElement = "Phys";
    public const string GunElement = "Gun";
    public const string FireElement = "Fire";
    public const string IceElement = "Ice";
    public const string ElecElement = "Elec";
    public const string ForceElement = "Force";
    public const string LightElement = "Light";
    public const string DarkElement = "Dark";
    public const string AlmightyElement = "Almighty";
    
    public const double WeakDamageMultiplier = 1.5;
    public const double ResistDamageMultiplier = 0.5;
    public const double NullDamageMultiplier = 0;
    public const double DrainDamageMultiplier = -1;
}