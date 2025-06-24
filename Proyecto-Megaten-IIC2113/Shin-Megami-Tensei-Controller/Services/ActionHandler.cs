using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei.Models;
using Shin_Megami_Tensei.Utilities;
using Shin_Megami_Tensei.Services;

namespace Shin_Megami_Tensei.Services;

public class ActionHandler
{
    private readonly View _view;
    private readonly CombatService _combatService;
    private readonly GameDisplayService _gameDisplayService;
    private readonly TargetSelector _targetSelector;
    private readonly SkillHandler _skillHandler;
    
    public ActionHandler(View view, CombatService combatService, GameDisplayService gameDisplayService, SkillLoader skillLoader)
    {
        _view = view;
        _combatService = combatService;
        _gameDisplayService = gameDisplayService;
        _targetSelector = new TargetSelector(view);
        _skillHandler = new SkillHandler(view, combatService, gameDisplayService, _targetSelector, skillLoader);
    }
    
    public ActionResult ExecuteAction(string action, Unit unit)
    {
        return action switch
        {
            "Atacar" => HandleAttack(unit),
            "Disparar" => HandleShoot(unit),
            "Usar Habilidad" => _skillHandler.HandleSkillAction(unit),
            "Invocar" => HandleInvoke(unit),
            "Pasar Turno" => HandlePassTurn(),
            "Rendirse" => HandleSurrender(),
            _ => new ActionResult { Success = false, Message = "Opción no implementada" }
        };
    }
    
    private ActionResult HandleAttack(Unit attacker)
    {
        Unit target = _targetSelector.SelectEnemyTarget(_combatService.OpponentTeam, attacker.Name);
        if (target == null) return new ActionResult { Cancelled = true };
        
        _view.WriteLine("----------------------------------------");
        return ExecuteAndDisplayResult(_combatService.ExecuteAttack(attacker, target));
    }
    
    private ActionResult HandleShoot(Unit attacker)
    {
        if (!(attacker is Samurai))
            return new ActionResult { Success = false, Message = "Solo los samurai pueden disparar" };
        
        Unit target = _targetSelector.SelectEnemyTarget(_combatService.OpponentTeam, attacker.Name);
        if (target == null) return new ActionResult { Cancelled = true };
        
        _view.WriteLine("----------------------------------------");
        return ExecuteAndDisplayResult(_combatService.ExecuteShoot(attacker, target));
    }
    
    private ActionResult HandleInvoke(Unit summoner)
    {
        return new InvokeHandler(_view, _combatService, _gameDisplayService).HandleInvoke(summoner);
    }
    
    private ActionResult HandlePassTurn()
    {
        ActionResult result = _combatService.ExecutePassTurn();
        _gameDisplayService.DisplayTurnConsumption(result.TurnConsumptionResult);
        return result;
    }
    
    private ActionResult HandleSurrender()
    {
        Team currentTeam = _combatService.CurrentTeam;
        Team opponentTeam = _combatService.OpponentTeam;
        
        _view.WriteLine("----------------------------------------");
        _view.WriteLine($"{currentTeam.Samurai.Name} ({currentTeam.PlayerName}) se rinde");

        return new ActionResult 
        { 
            Success = true, 
            GameEnded = true, 
            Winner = opponentTeam.PlayerName
        };
    }
    
    private ActionResult ExecuteAndDisplayResult(ActionResult result)
    {
        foreach (var message in result.Messages)
        {
            _view.WriteLine(message);
        }
        _gameDisplayService.DisplayTurnConsumption(result.TurnConsumptionResult);
        return result;
    }
}