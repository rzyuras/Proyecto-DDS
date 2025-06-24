using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei.Models;
using Shin_Megami_Tensei.Utilities;
using Shin_Megami_Tensei.Services;

namespace Shin_Megami_Tensei.Services;

public class UserInputHandler
{
    private readonly View _view;
    private readonly CombatService _combatService;
    private readonly GameDisplayService _gameDisplayService;
    private readonly ActionHandler _actionHandler;
    
    public UserInputHandler(View view, CombatService combatService, GameDisplayService gameDisplayService, SkillLoader skillLoader)
    {
        _view = view;
        _combatService = combatService;
        _gameDisplayService = gameDisplayService;
        _actionHandler = new ActionHandler(view, combatService, gameDisplayService, skillLoader);
    }
    
    public ActionResult ProcessUnitTurn(Unit unit)
    {
        while (true)
        {
            DisplayActionMenu(unit);
            
            int actionIndex = GetActionIndex();
            if (IsInvalidActionIndex(actionIndex, unit.GetAvailableActions().Count))
            {
                continue;
            }
            
            string selectedAction = unit.GetAvailableActions()[actionIndex];
            ActionResult result = _actionHandler.ExecuteAction(selectedAction, unit);
            
            if (result != null && !result.Cancelled)
            {
                return result;
            }
        }
    }
    
    private void DisplayActionMenu(Unit unit)
    {
        _view.WriteLine("----------------------------------------");
        _view.WriteLine($"Seleccione una acción para {unit.Name}");
        
        List<string> availableActions = unit.GetAvailableActions();
        for (int i = 0; i < availableActions.Count; i++)
        {
            _view.WriteLine($"{i + 1}: {availableActions[i]}");
        }
    }
    
    private int GetActionIndex()
    {
        return int.Parse(_view.ReadLine()) - 1;
    }
    
    private bool IsInvalidActionIndex(int index, int maxActions)
    {
        return index < 0 || index >= maxActions;
    }
}