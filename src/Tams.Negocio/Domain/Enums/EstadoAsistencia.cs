namespace Tams.Negocio.Domain.Enums;

/// <summary>
/// RF-NEG-01: estado de la asistencia de un estudiante en una clase.
/// Declarado en un único lugar del módulo de negocio.
/// </summary>
public enum EstadoAsistencia
{
    Presente = 0,
    Ausente = 1,
    Tardanza = 2,
    Excusa = 3,
}
