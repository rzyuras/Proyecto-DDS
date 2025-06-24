using System.Text.Json.Serialization;

namespace Shin_Megami_Tensei.Models;

public class Monster : Unit
{
    public List<string> SkillNames { get; set; } = new List<string>();
        
    public override List<string> GetAvailableActions()
    {
        return new List<string> { "Atacar", "Usar Habilidad", "Invocar", "Pasar Turno" };
    }
}