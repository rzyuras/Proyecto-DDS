namespace Shin_Megami_Tensei.Models;

public class Samurai : Unit
{
    public List<string> SkillNames { get; set; } = new List<string>();
        
    public override List<string> GetAvailableActions()
    {
        return new List<string> { "Atacar", "Disparar", "Usar Habilidad", "Invocar", "Pasar Turno", "Rendirse" };
    }
}