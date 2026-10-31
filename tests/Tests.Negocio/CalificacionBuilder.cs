using Tams.Negocio.Domain.Entidades;
using Tams.Negocio.Domain.Enums;

namespace Tests.Negocio;

/// <summary>
/// Construye calificaciones en memoria para probar la máquina de estados sin base de datos.
/// </summary>
internal static class CalificacionBuilder
{
    public static Calificacion Crear(
        EstadoCalificacion estado,
        decimal puntajeObtenido = 70m,
        decimal? puntajeRecuperacion = null) =>
        new()
        {
            Estado = estado,
            PuntajeObtenido = puntajeObtenido,
            PuntajeRecuperacion = puntajeRecuperacion,
        };
}
