namespace Shin_Megami_Tensei.Common;

public class GameMessages
{
    // Mensajes generales
    public const string SelectTeamFile = "Elige un archivo para cargar los equipos";
    public const string InvalidSelection = "Selección inválida";
    public const string InvalidTeamFile = "Archivo de equipos no válido";
    
    // Mensajes de combate
    public const string RoundFormat = "Ronda de {0} ({1})";
    public const string TeamFormat = "Equipo de {0} ({1})";
    public const string TurnInfoFormat = "Full Turns: {0}\nBlinking Turns: {1}";
    public const string ActionOrderHeader = "Orden:";
    public const string ActionOrderFormat = "{0}- {1}";
    public const string SelectActionFormat = "Seleccione una acción para {0}";
    public const string SelectTargetFormat = "Seleccione un objetivo para {0}";
    public const string SelectSkillFormat = "Seleccione una habilidad para que {0} use";
    public const string CancelOption = "Cancelar";
    public const string ActionCancelled = "Acción cancelada";
    public const string InvalidTarget = "Objetivo inválido";
    public const string TurnPassed = "Turno pasado";
    public const string FeatureNotImplemented = "Opción no implementada en esta entrega";
    public const string OnlySamuraiCanShoot = "Solo los samurai pueden disparar";
    
    // Mensajes de resultado de acción
    public const string AttackFormat = "{0} ataca a {1}\n{1} recibe {2} de daño\n{1} termina con HP:{3}/{4}";
    public const string ShootFormat = "{0} dispara a {1}\n{1} recibe {2} de daño\n{1} termina con HP:{3}/{4}";
    public const string SurrenderFormat = "{0} se rinde";
    public const string WinnerFormat = "Ganador: {0}";
    
    // Mensajes de consumo de turnos
    public const string TurnsConsumedFormat = "Se han consumido {0} Full Turn(s) y {1} Blinking Turn(s)";
    public const string TurnsGainedFormat = "Se han obtenido {0} Blinking Turn(s)";
}