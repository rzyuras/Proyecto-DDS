using Shin_Megami_Tensei.Models;
using Shin_Megami_Tensei.Utilities;

namespace Shin_Megami_Tensei.Validation;

public class TeamValidator
{
    private const int MAX_MONSTERS_PER_TEAM = 7;
    private const int MAX_SKILLS_PER_SAMURAI = 8;
    private readonly FileLoader _fileLoader;
        
    public TeamValidator(FileLoader fileLoader)
    {
        _fileLoader = fileLoader;
    }
        
    public bool IsValid(Team team)
    {
        if (team == null)
            return false;
        
        if (team.Samurai == null)
            return false;
        
        if (team.Monsters.Count > MAX_MONSTERS_PER_TEAM)
            return false;
        
        if (HasDuplicateMonsters(team))
            return false;
        
        if (HasTooManySkills(team.Samurai))
            return false;
        
        if (HasDuplicateSkills(team.Samurai))
            return false;
        
        return true;
    }
        
    private bool HasDuplicateMonsters(Team team)
    {
        return team.Monsters
            .Select(m => m.Name)
            .Distinct()
            .Count() != team.Monsters.Count;
    }
        
    private bool HasTooManySkills(Samurai samurai)
    {
        return samurai.SkillNames.Count > MAX_SKILLS_PER_SAMURAI;
    }
        
    private bool HasDuplicateSkills(Samurai samurai)
    {
        return samurai.SkillNames
            .Distinct()
            .Count() != samurai.SkillNames.Count;
    }
        
    private bool HasInvalidSkills(Samurai samurai)
    {
        return samurai.SkillNames.Any(skillName => !_fileLoader.IsValidSkillName(skillName));
    }
}