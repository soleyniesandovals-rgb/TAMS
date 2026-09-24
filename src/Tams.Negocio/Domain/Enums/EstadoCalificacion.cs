namespace Tams.Negocio.Domain.Enums;

/// <summary>
/// RD-04 / RF-NEG-03: estado del ciclo de vida de una calificación.
/// Declarado en un único lugar del módulo de negocio.
/// Las transiciones entre estados se implementan en Fase 1 (S4).
/// </summary>
public enum EstadoCalificacion
{
    Borrador = 0,
    Enviada = 1,
    Publicada = 2,
    EnRecuperacion = 3,
    Cerrada = 4,
}