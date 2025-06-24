namespace Shin_Megami_Tensei.Models;
using Shin_Megami_Tensei.Common;

public class Team
{
    public string PlayerName { get; set; }
    public Samurai Samurai { get; set; }
    public List<Monster> Monsters { get; set; } = new List<Monster>();
    public Unit[] BoardPositions { get; private set; } = new Unit[GameConstants.BoardPositionsPerTeam];
    public List<Monster> Reserve { get; private set; } = new List<Monster>();
    public int SkillUsageCount { get; set; } = 0;
        
    public void InitializeBoard()
    {
        BoardPositions[GameConstants.InitialPositionForSamurai] = Samurai;
        int monstersToPlace = Math.Min(GameConstants.BoardPositionsPerTeam - 1, Monsters.Count);
        for (int i = 0; i < monstersToPlace; i++)
        {
            BoardPositions[i + 1] = Monsters[i];
        }

        Reserve = new List<Monster>();
        for (int i = monstersToPlace; i < Monsters.Count; i++)
        {
            Reserve.Add(Monsters[i]);
        }
    }
        
    public int CountAliveUnits()
    {
        return BoardPositions.Count(unit => unit != null && unit.IsAlive);
    }
        
    public bool HasAliveUnits()
    {
        return CountAliveUnits() > 0;
    }
}