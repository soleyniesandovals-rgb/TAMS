using Tams.Negocio.Application.Modelos;

namespace Tams.Negocio.Application.Servicios;

/// <summary>
/// Consultas de solo lectura para el docente autenticado (RF-NEG-02, RD-02, RD-06).
/// </summary>
public interface IConsultasDocenteServicio
{
    /// <summary>Lista las asignaciones del docente con datos de materia, curso y año escolar.</summary>
    Task<IReadOnlyList<AsignacionDocenteResumen>> ListarMisAsignacionesAsync(
        int usuarioId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista los estudiantes de un curso si el docente tiene alguna asignación en él.
    /// </summary>
    Task<IReadOnlyList<EstudianteResumen>> ListarEstudiantesDeCursoAsync(
        int usuarioId,
        int cursoId,
        CancellationToken cancellationToken = default);

    /// <summary>Obtiene el año escolar activo.</summary>
    Task<AnioEscolarResumen> ObtenerAnioEscolarActivoAsync(
        CancellationToken cancellationToken = default);
}
