using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei.Models;
using Shin_Megami_Tensei.Services;

namespace Shin_Megami_Tensei.Services;

public class GameDisplayService
{
    private readonly View _view;
    
    public GameDisplayService(View view)
    {
        _view = view;
    }
    
    public void DisplayTeamOptions(string[] teamFiles)
    {
        _view.WriteLine("Elige un archivo para cargar los equipos");
        
        for (int i = 0; i < teamFiles.Length; i++)
        {
            _view.WriteLine($"{i}: {teamFiles[i]}");
        }
    }
    
    public void DisplayRoundInfo(Team team)
    {
        _view.WriteLine("----------------------------------------");
        _view.WriteLine($"Ronda de {team.Samurai.Name} ({team.PlayerName})");
    }
    
    public void DisplayGameState(Team currentTeam, Team opponentTeam, TurnInfo turnInfo, List<Unit> actionOrder)
    {
        DisplayBoardState(currentTeam, opponentTeam);
        DisplayTurnsInfo(turnInfo);
        DisplayActionOrder(actionOrder);
    }
    
    private void DisplayBoardState(Team currentTeam, Team opponentTeam)
    {
        Team player1Team = currentTeam.PlayerName == "J1" ? currentTeam : opponentTeam;
        Team player2Team = currentTeam.PlayerName == "J1" ? opponentTeam : currentTeam;
        
        _view.WriteLine("----------------------------------------");

        _view.WriteLine($"Equipo de {player1Team.Samurai.Name} ({player1Team.PlayerName})");
        DisplayTeamPositions(player1Team);

        _view.WriteLine($"Equipo de {player2Team.Samurai.Name} ({player2Team.PlayerName})");
        DisplayTeamPositions(player2Team);
    }
    
    private void DisplayTeamPositions(Team team)
    {
        char position = 'A';
        foreach (var unit in team.BoardPositions)
        {
            DisplayUnitPosition(unit, position);
            position++;
        }
    }
    
    private void DisplayUnitPosition(Unit unit, char position)
    {
        if (unit == null || (!unit.IsAlive && !(unit is Samurai)))
        {
            _view.WriteLine($"{position}-");
        }
        else
        {
            _view.WriteLine($"{position}-{unit.Name} HP:{unit.CurrentHP}/{unit.MaxHP} MP:{unit.CurrentMP}/{unit.MaxMP}");
        }
    }
    
    private void DisplayTurnsInfo(TurnInfo turnInfo)
    {
        _view.WriteLine("----------------------------------------");
        _view.WriteLine($"Full Turns: {turnInfo.FullTurns}");
        _view.WriteLine($"Blinking Turns: {turnInfo.BlinkingTurns}");
    }
    
    private void DisplayActionOrder(List<Unit> units)
    {
        _view.WriteLine("----------------------------------------");
        _view.WriteLine("Orden:");
        for (int i = 0; i < units.Count; i++)
        {
            _view.WriteLine($"{i+1}-{units[i].Name}");
        }
    }
    
    public void DisplayTurnConsumption(TurnConsumptionResult turnResult)
    {
        _view.WriteLine("----------------------------------------");
        _view.WriteLine($"Se han consumido {turnResult.FullTurnsConsumed} Full Turn(s) y {turnResult.BlinkingTurnsConsumed} Blinking Turn(s)");
        _view.WriteLine($"Se han obtenido {turnResult.BlinkingTurnsGained} Blinking Turn(s)");
    }
    
    public void DisplayWinner(string winnerName, Team currentTeam, Team opponentTeam)
    {
        _view.WriteLine("----------------------------------------");

        Team winnerTeam = currentTeam.PlayerName == winnerName ? currentTeam : opponentTeam;
        _view.WriteLine($"Ganador: {winnerTeam.Samurai.Name} ({winnerTeam.PlayerName})");
    }
}