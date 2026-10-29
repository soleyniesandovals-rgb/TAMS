namespace Tams.Negocio.Domain;

/// <summary>
/// Reglas numéricas compartidas por la máquina de estados y los servicios de aplicación
/// (RF-NEG-03, RF-NEG-04). Los valores viven aquí, en un único lugar, para que no se
/// repitan ni en las transiciones ni en los servicios (RD-04).
/// </summary>
public static class ReglasCalificacion
{
    /// <summary>Mínimo de aprobación por defecto.</summary>
    public const decimal MinimoAprobacion = 70m;

    /// <summary>Puntaje máximo de la escala, incluido.</summary>
    public const decimal PuntajeMaximo = 100m;
}
