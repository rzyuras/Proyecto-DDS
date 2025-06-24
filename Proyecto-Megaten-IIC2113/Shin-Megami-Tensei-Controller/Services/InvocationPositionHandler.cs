using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei.Models;
using Shin_Megami_Tensei.Services;

namespace Shin_Megami_Tensei.Services;

public class InvocationPositionHandler
{
    private readonly View _view;
    private readonly CombatService _combatService;
    private readonly GameDisplayService _gameDisplayService;
    private readonly TargetSelector _targetSelector;
    
    public InvocationPositionHandler(View view, CombatService combatService, GameDisplayService gameDisplayService)
    {
        _view = view;
        _combatService = combatService;
        _gameDisplayService = gameDisplayService;
        _targetSelector = new TargetSelector(view);
    }
    
    public ActionResult HandleInvocationPosition(Unit summoner, Monster monster, Skill skill)
    {
        if (IsSkillBasedInvocation(skill))
        {
            return HandleSkillBasedInvocation(summoner, monster, skill);
        }
        
        if (summoner is Monster)
        {
            return HandleMonsterInvocation(summoner, monster, skill);
        }
        
        return new ActionResult { Success = false, Message = "Invalid invocation" };
    }
    
    private bool IsSkillBasedInvocation(Skill skill)
    {
        return skill != null && (skill.Name == "Sabbatma" || skill.Name == "Invitation");
    }
    
    private ActionResult HandleSkillBasedInvocation(Unit summoner, Monster monster, Skill skill)
    {
        int boardPosition = _targetSelector.SelectBoardPosition(_combatService.CurrentTeam);
        
        if (boardPosition == -1) return new ActionResult { Cancelled = true };
        if (boardPosition == -2) return new ActionResult { Success = false, Message = "Posición inválida" };
        
        _view.WriteLine("----------------------------------------");
        
        ActionResult result = _combatService.ExecuteSkill(summoner, monster, skill);
        
        if (result.Success && result.SpecialAction == "Summon")
        {
            PlaceMonsterOnBoard(monster, boardPosition);
        }
        
        DisplayResultMessages(result);
        return result;
    }
    
    private ActionResult HandleMonsterInvocation(Unit summoner, Monster monster, Skill skill)
    {
        _view.WriteLine("----------------------------------------");
        
        ActionResult result = _combatService.ExecuteSkill(summoner, monster, skill);
        
        if (result.SpecialAction == "Summon")
        {
            ReplaceMonsterOnBoard(summoner, monster);
        }
        
        DisplayResultMessages(result);
        return result;
    }
    
    private void PlaceMonsterOnBoard(Monster monster, int boardPosition)
    {
        Team team = _combatService.CurrentTeam;
        
        if (team.BoardPositions[boardPosition] == null)
        {
            PlaceMonsterInEmptyPosition(monster, boardPosition, team);
        }
        else
        {
            ReplaceExistingMonster(monster, boardPosition, team);
        }
    }
    
    private void PlaceMonsterInEmptyPosition(Monster monster, int boardPosition, Team team)
    {
        team.BoardPositions[boardPosition] = monster;
        team.Reserve.Remove(monster);
        _combatService.ActionOrder.Add(monster);
    }
    
    private void ReplaceExistingMonster(Monster monster, int boardPosition, Team team)
    {
        Monster existingMonster = team.BoardPositions[boardPosition] as Monster;
        if (existingMonster != null)
        {
            team.BoardPositions[boardPosition] = monster;
            team.Reserve.Remove(monster);
            team.Reserve.Add(existingMonster);
            
            UpdateActionOrder(existingMonster, monster);
        }
    }
    
    private void ReplaceMonsterOnBoard(Unit summoner, Monster monster)
    {
        Team team = _combatService.CurrentTeam;
        int summonerPosition = Array.IndexOf(team.BoardPositions, summoner);
        
        if (summonerPosition >= 0)
        {
            team.BoardPositions[summonerPosition] = monster;
            team.Reserve.Remove(monster);
            team.Reserve.Add(summoner as Monster);
            
            UpdateActionOrder(summoner, monster);
        }
    }
    
    private void UpdateActionOrder(Unit oldUnit, Unit newUnit)
    {
        var actionOrder = _combatService.ActionOrder;
        int orderIndex = actionOrder.IndexOf(oldUnit);
        if (orderIndex >= 0)
        {
            actionOrder[orderIndex] = newUnit;
        }
    }
    
    private void DisplayResultMessages(ActionResult result)
    {
        foreach (var message in result.Messages)
        {
            _view.WriteLine(message);
        }
        
        _gameDisplayService.DisplayTurnConsumption(result.TurnConsumptionResult);
    }
}