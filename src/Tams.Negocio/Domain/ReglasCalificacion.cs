using Tams.Negocio.Domain.Enums;

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

    /// <summary>
    /// Indica si un estado significa que la nota ya pasó por la publicación.
    /// Cuentan <see cref="Enums.EstadoCalificacion.Publicada"/>,
    /// <see cref="Enums.EstadoCalificacion.EnRecuperacion"/> y
    /// <see cref="Enums.EstadoCalificacion.Cerrada"/>; no cuentan Borrador ni Enviada.
    /// Se usa al registrar la nota siguiente (RF-NEG, RD-02).
    /// </summary>
    public static bool EstaPublicada(EstadoCalificacion estado)
        => estado is EstadoCalificacion.Publicada
            or EstadoCalificacion.EnRecuperacion
            or EstadoCalificacion.Cerrada;

    /// <summary>
    /// Indica si el puntaje todavía se puede cambiar sin permiso y sin transición de estado:
    /// solo mientras la calificación está en <see cref="Enums.EstadoCalificacion.Borrador"/>
    /// (RF-NEG-02). No sustituye a la máquina de estados; la edición no cambia el estado.
    /// </summary>
    public static bool EsEditable(EstadoCalificacion estado)
        => estado == EstadoCalificacion.Borrador;

    /// <summary>
    /// Advertencia informativa cuando una nota de RA queda por debajo del mínimo definido
    /// para ese resultado de aprendizaje (RF-NEG, RD-02). No bloquea ni cambia la nota.
    /// </summary>
    public static string? AdvertenciaMinimoRa(int numeroRa, decimal puntaje, int? puntajeMinimo)
    {
        if (puntajeMinimo is null || puntaje >= puntajeMinimo.Value)
        {
            return null;
        }

        return $"La nota del RA{numeroRa} está por debajo del mínimo de {puntajeMinimo.Value} puntos.";
    }
}
