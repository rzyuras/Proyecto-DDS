namespace Shin_Megami_Tensei.Models;
using Shin_Megami_Tensei.Services;
using Shin_Megami_Tensei.Common;

public abstract class Skill
{
    public string Name { get; set; }
    public string Type { get; set; }
    public int Cost { get; set; }
    public int Power { get; set; }
    public string Target { get; set; }
    public string Hits { get; set; }
    public string Effect { get; set; }
    
    public abstract ActionResult Execute(Unit user, Unit target, Team userTeam, Team targetTeam, 
        TurnManager turnManager, DamageCalculator damageCalculator);
                                        
    protected int GetHitCount(int userSkillUsageCount)
    {
        if (string.IsNullOrEmpty(Hits) || Hits == "1")
            return 1;

        if (Hits.Contains('-'))
        {
            string[] parts = Hits.Split('-');
            if (parts.Length == 2 && 
                int.TryParse(parts[0].Trim(), out int min) && 
                int.TryParse(parts[1].Trim(), out int max))
            {
                int offset = userSkillUsageCount % (max - min + 1);
                return min + offset;
            }
        }
        
        string cleanHits = Hits.Trim('[', ']', ' ');
        if (cleanHits.Contains(','))
        {
            string[] parts = cleanHits.Split(',');
            if (parts.Length == 2 && 
                int.TryParse(parts[0].Trim(), out int min) && 
                int.TryParse(parts[1].Trim(), out int max))
            {
                int offset = userSkillUsageCount % (max - min + 1);
                return min + offset;
            }
        }
        
        if (int.TryParse(cleanHits, out int singleHit))
        {
            return singleHit;
        }
        
        return 1;
    }
}