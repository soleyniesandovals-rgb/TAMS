namespace Tams.Negocio.Domain;

/// <summary>
/// Reglas numéricas de las asignaciones docentes (RF-NEG-02).
/// El límite de horas se cambia aquí; la base de datos solo exige que sea mayor que cero.
/// </summary>
public static class ReglasAsignacion
{
    /// <summary>Horas semanales mínimas que puede tener una asignación.</summary>
    public const int HorasSemanalesMinimas = 1;

    /// <summary>Horas semanales máximas que puede tener una asignación.</summary>
    public const int HorasSemanalesMaximas = 40;
}
