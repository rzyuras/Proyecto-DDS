namespace Shin_Megami_Tensei.Models.JSON;

public class SamuraiData
{
    public string Name { get; set; }
    public StatsData Stats { get; set; }
    public Dictionary<string, string> Affinity { get; set; }
}