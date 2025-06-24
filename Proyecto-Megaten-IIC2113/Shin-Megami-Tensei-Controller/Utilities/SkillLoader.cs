namespace Shin_Megami_Tensei.Utilities;
using Shin_Megami_Tensei.Models;
using Shin_Megami_Tensei.Common;
using Shin_Megami_Tensei.Models.JSON;
using System.Text.Json;

public class SkillLoader
{
    private readonly string _skillsFilePath;
    private Dictionary<string, Skill> _skillsCache = new Dictionary<string, Skill>();
    
    public SkillLoader(string teamsFolder)
    {
        string dataFolder = Path.GetDirectoryName(teamsFolder) ?? teamsFolder;
        _skillsFilePath = Path.Combine(dataFolder, "skills.json");
        LoadSkills();
    }
    
    private void LoadSkills()
    {
        if (!File.Exists(_skillsFilePath))
            throw new FileNotFoundException($"Skills file not found: {_skillsFilePath}");
            
        string json = File.ReadAllText(_skillsFilePath);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var skillsData = JsonSerializer.Deserialize<List<SkillData>>(json, options);
        
        foreach (var skillData in skillsData)
        {
            Skill skill = CreateSkillFromData(skillData);
            if (skill != null)
            {
                _skillsCache[skillData.Name] = skill;
            }
        }
    }
    
    private Skill CreateSkillFromData(SkillData skillData)
    {
        Skill skill = null;

        if (skillData.Name == "Invitation")
        {
            skill = new InvitationSkill();
        }
        else if (skillData.Name == "Sabbatma")
        {
            skill = new SabbatmaSkill();
        }
        else if (skillData.Name == "Recarm" || skillData.Name == "Samarecarm")
        {
            skill = new RecarmSkill();
        }
        else if (IsOffensiveSkill(skillData.Type))
        {
            skill = new OffensiveSkill();
        }
        else if (skillData.Type == "Heal")
        {
            skill = new BasicHealSkill();
        }
        else if (skillData.Type == "Special")
        {
            skill = new SpecialSkill();
        }
        else if (skillData.Type == "Support")
        {
            skill = new SpecialSkill();
        }
        else if (skillData.Type == "Ailment")
        {
            skill = new SpecialSkill();
        }
        else if (skillData.Type == "Passive")
        {
            skill = new SpecialSkill();
        }
        else
        {
            return null;
        }

        skill.Name = skillData.Name;
        skill.Type = skillData.Type;
        skill.Cost = skillData.Cost;
        skill.Power = skillData.Power;
        skill.Target = skillData.Target;
        skill.Hits = skillData.Hits;
        skill.Effect = skillData.Effect;
        
        return skill;
    }
    
    private bool IsOffensiveSkill(string type)
    {
        return type == GameConstants.PhysElement ||
               type == GameConstants.GunElement ||
               type == GameConstants.FireElement ||
               type == GameConstants.IceElement ||
               type == GameConstants.ElecElement ||
               type == GameConstants.ForceElement ||
               type == GameConstants.LightElement ||
               type == GameConstants.DarkElement ||
               type == GameConstants.AlmightyElement;
    }
    
    public Skill GetSkill(string skillName)
    {
        if (_skillsCache.TryGetValue(skillName, out Skill skill))
            return skill;
            
        return null;
    }
    
    public List<Skill> GetSkills(List<string> skillNames)
    {
        return skillNames
            .Where(name => _skillsCache.ContainsKey(name))
            .Select(name => _skillsCache[name])
            .ToList();
    }
}