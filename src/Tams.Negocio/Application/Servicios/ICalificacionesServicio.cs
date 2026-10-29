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
}
