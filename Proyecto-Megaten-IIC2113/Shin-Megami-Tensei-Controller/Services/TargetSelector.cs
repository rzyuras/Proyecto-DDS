using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei.Models;

namespace Shin_Megami_Tensei.Services;

public class TargetSelector
{
    private readonly View _view;
    
    public TargetSelector(View view)
    {
        _view = view;
    }
    
    public Unit SelectEnemyTarget(Team enemyTeam, string attackerName)
    {
        _view.WriteLine("----------------------------------------");
        _view.WriteLine($"Seleccione un objetivo para {attackerName}");

        List<Unit> possibleTargets = GetAliveUnits(enemyTeam);
        DisplayTargets(possibleTargets);
        
        int selectedIndex = GetTargetIndex(possibleTargets.Count);
        return selectedIndex == -1 ? null : possibleTargets[selectedIndex];
    }
    
    public Unit SelectAllyTarget(Team allyTeam, string userInitiator, bool includeDeadUnits = false)
    {
        _view.WriteLine("----------------------------------------");
        _view.WriteLine($"Seleccione un objetivo para {userInitiator}");
        
        List<Unit> possibleTargets = includeDeadUnits ? 
            GetDeadUnits(allyTeam) : 
            GetAliveUnits(allyTeam);
            
        DisplayTargets(possibleTargets);
        
        int selectedIndex = GetTargetIndex(possibleTargets.Count);
        return selectedIndex == -1 ? null : possibleTargets[selectedIndex];
    }
    
    public Monster SelectMonsterFromReserve(Team team, string prompt, bool aliveOnly = false)
    {
        _view.WriteLine("----------------------------------------");
        _view.WriteLine(prompt);
        
        List<Monster> possibleMonsters = aliveOnly ?
            GetAliveReserveMonsters(team) :
            GetAllReserveMonsters(team);
            
        if (possibleMonsters.Count == 0)
        {
            DisplayNoMonstersMessage();
            return null;
        }
        
        DisplayMonsters(possibleMonsters);
        
        int selectedIndex = GetTargetIndex(possibleMonsters.Count);
        return selectedIndex == -1 ? null : possibleMonsters[selectedIndex];
    }
    
    public int SelectBoardPosition(Team team)
    {
        _view.WriteLine("----------------------------------------");
        _view.WriteLine("Seleccione una posición para invocar");
        
        DisplayBoardPositions(team);
        
        int positionIndex = int.Parse(_view.ReadLine());
        
        if (positionIndex == team.BoardPositions.Length)
        {
            return -1;
        }
        
        if (IsValidBoardPosition(positionIndex, team.BoardPositions.Length))
        {
            return positionIndex;
        }
        
        return -2;
    }
    
    private List<Unit> GetAliveUnits(Team team)
    {
        return team.BoardPositions
            .Where(u => u != null && u.IsAlive)
            .ToList();
    }
    
    private List<Unit> GetDeadUnits(Team team)
    {
        var deadOnBoard = team.BoardPositions
            .Where(u => u != null && !u.IsAlive)
            .ToList();
        var deadInReserve = team.Reserve
            .Where(u => !u.IsAlive)
            .Cast<Unit>()
            .ToList();
        
        return deadOnBoard.Concat(deadInReserve).ToList();
    }
    
    private List<Monster> GetAliveReserveMonsters(Team team)
    {
        return team.Monsters
            .Where(m => team.Reserve.Contains(m) && m.IsAlive)
            .ToList();
    }
    
    private List<Monster> GetAllReserveMonsters(Team team)
    {
        return team.Monsters
            .Where(m => team.Reserve.Contains(m))
            .ToList();
    }
    
    private void DisplayTargets(List<Unit> targets)
    {
        for (int i = 0; i < targets.Count; i++)
        {
            Unit target = targets[i];
            _view.WriteLine($"{i + 1}-{target.Name} HP:{target.CurrentHP}/{target.MaxHP} MP:{target.CurrentMP}/{target.MaxMP}");
        }
        
        _view.WriteLine($"{targets.Count + 1}-Cancelar");
    }
    
    private void DisplayMonsters(List<Monster> monsters)
    {
        for (int i = 0; i < monsters.Count; i++)
        {
            Monster monster = monsters[i];
            _view.WriteLine($"{i + 1}-{monster.Name} HP:{monster.CurrentHP}/{monster.MaxHP} MP:{monster.CurrentMP}/{monster.MaxMP}");
        }
        
        _view.WriteLine($"{monsters.Count + 1}-Cancelar");
    }
    
    private void DisplayBoardPositions(Team team)
    {
        for (int i = 1; i < team.BoardPositions.Length; i++)
        {
            if (team.BoardPositions[i] == null)
            {
                _view.WriteLine($"{i}-Vacío (Puesto {i+1})");
            }
            else
            {
                Unit unit = team.BoardPositions[i];
                _view.WriteLine($"{i}-{unit.Name} HP:{unit.CurrentHP}/{unit.MaxHP} MP:{unit.CurrentMP}/{unit.MaxMP} (Puesto {i+1})");
            }
        }
        
        _view.WriteLine($"{team.BoardPositions.Length}-Cancelar");
    }
    
    private void DisplayNoMonstersMessage()
    {
        _view.WriteLine("No hay monstruos disponibles en reserva");
        _view.WriteLine("1-Cancelar");
        _view.ReadLine();
    }
    
    private int GetTargetIndex(int maxTargets)
    {
        int selectedIndex = int.Parse(_view.ReadLine()) - 1;
        
        if (selectedIndex == maxTargets)
        {
            return -1;
        }
        
        if (selectedIndex < 0 || selectedIndex >= maxTargets)
        {
            return -1;
        }
        
        return selectedIndex;
    }
    
    private bool IsValidBoardPosition(int position, int maxPositions)
    {
        return position >= 1 && position < maxPositions;
    }
}