namespace Shin_Megami_Tensei.Models.JSON;

public class MonsterData
{
    public string Name { get; set; }
    public StatsData Stats { get; set; }
    public Dictionary<string, string> Affinity { get; set; }
    public List<string> Skills { get; set; } = new List<string>();
}