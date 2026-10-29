using Tams.Negocio.Application.Modelos;
using Tams.Negocio.Domain.Enums;

namespace Tams.Negocio.Application.Servicios;

/// <summary>
/// Operaciones de calificaciones del módulo de negocio (RF-NEG-02, RF-NEG-03, RF-NEG-04,
/// RF-NEG-05, RF-NEG-06). La lógica vive aquí y no en el controlador (RD-02); los estados
/// y sus condiciones se delegan a la máquina del dominio (RD-04).
/// </summary>
public interface ICalificacionesServicio
{
    /// <summary>
    /// Registra una calificación en estado <see cref="EstadoCalificacion.Borrador"/>
    /// (RF-NEG-02). Valida la entrada (RD-07), que existan el estudiante, la materia y el
    /// año escolar, que el usuario sea docente titular y que no exista otra calificación
    /// igual.
    /// </summary>
    Task<CalificacionResumen> RegistrarAsync(
        int usuarioId,
        int estudianteId,
        int materiaId,
        int anioEscolarId,
        TipoEvaluacion tipoEvaluacion,
        int? periodo,
        int? numeroRa,
        decimal puntajeObtenido,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Devuelve solo las calificaciones de las materias y cursos donde el usuario es
    /// docente titular (RF-NEG-02).
    /// </summary>
    Task<IReadOnlyList<CalificacionResumen>> ConsultarMisCalificacionesAsync(
        int usuarioId,
        CancellationToken cancellationToken = default);

    /// <summary>Envía una calificación en borrador (Borrador → Enviada, RF-NEG-03).</summary>
    Task<CalificacionResumen> EnviarAsync(
        int usuarioId,
        int calificacionId,
        CancellationToken cancellationToken = default);

    /// <summary>Publica una calificación enviada (Enviada → Publicada, RF-NEG-03).</summary>
    Task<CalificacionResumen> PublicarAsync(
        int usuarioId,
        int calificacionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Envía a recuperación una calificación publicada cuyo puntaje no alcanzó el mínimo
    /// (Publicada → EnRecuperacion, RF-NEG-03).
    /// </summary>
    Task<CalificacionResumen> EnviarARecuperacionAsync(
        int usuarioId,
        int calificacionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cierra una calificación publicada (Publicada → Cerrada) o una que estaba en
    /// recuperación (EnRecuperacion → Cerrada), registrando en este último caso el
    /// <paramref name="puntajeRecuperacion"/> (RF-NEG-03, RF-NEG-05).
    /// </summary>
    Task<CalificacionResumen> CerrarAsync(
        int usuarioId,
        int calificacionId,
        decimal? puntajeRecuperacion = null,
        CancellationToken cancellationToken = default);
}
