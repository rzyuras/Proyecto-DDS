using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei.Models;
using Shin_Megami_Tensei.Services;

namespace Shin_Megami_Tensei.Services;

public class InvokeHandler
{
    private readonly View _view;
    private readonly CombatService _combatService;
    private readonly GameDisplayService _gameDisplayService;
    private readonly TargetSelector _targetSelector;
    
    public InvokeHandler(View view, CombatService combatService, GameDisplayService gameDisplayService)
    {
        _view = view;
        _combatService = combatService;
        _gameDisplayService = gameDisplayService;
        _targetSelector = new TargetSelector(view);
    }
    
    public ActionResult HandleInvoke(Unit summoner)
    {
        Monster selectedMonster = _targetSelector.SelectMonsterFromReserve(
            _combatService.CurrentTeam, 
            "Seleccione un monstruo para invocar", 
            aliveOnly: true);
        
        if (selectedMonster == null) return new ActionResult { Cancelled = true };
        
        return summoner is Monster ? 
            HandleMonsterInvoke(summoner, selectedMonster) : 
            HandleSamuraiInvoke(summoner, selectedMonster);
    }
    
    private ActionResult HandleMonsterInvoke(Unit summoner, Monster monster)
    {
        _view.WriteLine("----------------------------------------");
        
        ActionResult result = _combatService.ExecuteInvoke(summoner, monster);
        
        _view.WriteLine($"{monster.Name} ha sido invocado");
        _gameDisplayService.DisplayTurnConsumption(result.TurnConsumptionResult);
        
        return result;
    }
    
    private ActionResult HandleSamuraiInvoke(Unit summoner, Monster monster)
    {
        int boardPosition = _targetSelector.SelectBoardPosition(_combatService.CurrentTeam);
        
        if (boardPosition == -1) return new ActionResult { Cancelled = true };
        if (boardPosition == -2) return new ActionResult { Success = false, Message = "Posición inválida" };
        
        _view.WriteLine("----------------------------------------");
        
        ActionResult result = _combatService.ExecuteInvoke(summoner, monster, boardPosition);
        
        _view.WriteLine($"{monster.Name} ha sido invocado");
        _gameDisplayService.DisplayTurnConsumption(result.TurnConsumptionResult);
        
        return result;
    }
}