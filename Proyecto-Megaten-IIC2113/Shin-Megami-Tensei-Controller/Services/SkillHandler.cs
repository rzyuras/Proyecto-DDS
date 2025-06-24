using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei.Models;
using Shin_Megami_Tensei.Services;
using Shin_Megami_Tensei.Utilities;

namespace Shin_Megami_Tensei.Services;

public class SkillHandler
{
    private readonly View _view;
    private readonly CombatService _combatService;
    private readonly GameDisplayService _gameDisplayService;
    private readonly TargetSelector _targetSelector;
    private readonly SkillLoader _skillLoader;
    
    public SkillHandler(View view, CombatService combatService, GameDisplayService gameDisplayService, TargetSelector targetSelector, SkillLoader skillLoader)
    {
        _view = view;
        _combatService = combatService;
        _gameDisplayService = gameDisplayService;
        _targetSelector = targetSelector;
        _skillLoader = skillLoader;
    }
    
    public ActionResult HandleSkillAction(Unit unit)
    {
        Console.WriteLine($"HandleSkillAction called for {unit.Name}");
        
        Skill selectedSkill = SelectSkill(unit);
        if (selectedSkill == null) return new ActionResult { Cancelled = true };
        
        return ExecuteSkillBasedOnType(unit, selectedSkill);
    }
    
    private Skill SelectSkill(Unit unit)
    {
        Console.WriteLine($"SelectSkill called for {unit.Name}");
        
        _view.WriteLine("----------------------------------------");
        _view.WriteLine($"Seleccione una habilidad para que {unit.Name} use");

        Console.WriteLine("About to call GetUsableSkills");
        List<Skill> usableSkills = GetUsableSkills(unit);
        Console.WriteLine($"Got {usableSkills.Count} usable skills");
        
        DisplaySkills(usableSkills);
        
        int skillIndex = GetSkillIndex(usableSkills.Count);
        return skillIndex == -1 ? null : usableSkills[skillIndex];
    }
    
    private List<Skill> GetUsableSkills(Unit unit)
    {
        return new List<Skill>();
    }
    
    private List<Skill> GetUnitSkills(Unit unit, SkillLoader skillLoader)
    {
        return unit switch
        {
            Samurai samurai => skillLoader.GetSkills(samurai.SkillNames),
            Monster monster => skillLoader.GetSkills(monster.SkillNames),
            _ => new List<Skill>()
        };
    }
    
    private void DisplaySkills(List<Skill> skills)
    {
        for (int i = 0; i < skills.Count; i++)
        {
            _view.WriteLine($"{i + 1}-{skills[i].Name} MP:{skills[i].Cost}");
        }
        
        _view.WriteLine($"{skills.Count + 1}-Cancelar");
    }
    
    private int GetSkillIndex(int maxSkills)
    {
        int skillIndex = int.Parse(_view.ReadLine()) - 1;
        
        if (skillIndex == maxSkills || skillIndex < 0 || skillIndex >= maxSkills)
        {
            return -1;
        }
        
        return skillIndex;
    }
    
    private ActionResult ExecuteSkillBasedOnType(Unit unit, Skill skill)
    {
        if (IsInvocationSkill(skill))
        {
            return HandleInvocationSkill(unit, skill);
        }
        
        if (IsOffensiveSkill(skill))
        {
            return HandleOffensiveSkill(unit, skill);
        }
        
        if (IsHealSkill(skill))
        {
            return HandleHealSkill(unit, skill);
        }
        
        return new ActionResult { Success = false, Message = "Tipo de habilidad no soportado" };
    }
    
    private bool IsInvocationSkill(Skill skill)
    {
        return skill.Name == "Invitation" || skill.Name == "Sabbatma";
    }
    
    private bool IsOffensiveSkill(Skill skill)
    {
        return skill is OffensiveSkill && skill.Target == "Single";
    }
    
    private bool IsHealSkill(Skill skill)
    {
        return skill is HealSkill && skill.Target == "Ally";
    }
    
    private ActionResult HandleInvocationSkill(Unit user, Skill skill)
    {
        bool aliveOnly = skill.Name == "Sabbatma";
        Monster target = _targetSelector.SelectMonsterFromReserve(_combatService.CurrentTeam, 
            "Seleccione un monstruo para invocar", aliveOnly);
        
        if (target == null) return new ActionResult { Cancelled = true };
        
        return new InvocationPositionHandler(_view, _combatService, _gameDisplayService)
            .HandleInvocationPosition(user, target, skill);
    }
    
    private ActionResult HandleOffensiveSkill(Unit attacker, Skill skill)
    {
        Unit target = _targetSelector.SelectEnemyTarget(_combatService.OpponentTeam, attacker.Name);
        if (target == null) return new ActionResult { Cancelled = true };
        
        return ExecuteAndDisplaySkillResult(attacker, target, skill);
    }
    
    private ActionResult HandleHealSkill(Unit user, Skill skill)
    {
        bool targetDeadUnits = IsRevivalSkill(skill);
        Unit target = _targetSelector.SelectAllyTarget(_combatService.CurrentTeam, user.Name, targetDeadUnits);
        
        if (target == null) return new ActionResult { Cancelled = true };
        
        return ExecuteAndDisplaySkillResult(user, target, skill);
    }
    
    private bool IsRevivalSkill(Skill skill)
    {
        return skill.Name.Contains("Recarm") || skill.Name.Contains("Samarecarm");
    }
    
    private ActionResult ExecuteAndDisplaySkillResult(Unit user, Unit target, Skill skill)
    {
        _view.WriteLine("----------------------------------------");
        
        ActionResult result = _combatService.ExecuteSkill(user, target, skill);
        
        foreach (var message in result.Messages)
        {
            _view.WriteLine(message);
        }
        
        _gameDisplayService.DisplayTurnConsumption(result.TurnConsumptionResult);
        
        return result;
    }
}