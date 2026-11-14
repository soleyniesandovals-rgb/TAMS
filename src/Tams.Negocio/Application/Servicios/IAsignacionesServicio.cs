using Tams.Negocio.Application.Modelos;

namespace Tams.Negocio.Application.Servicios;

/// <summary>
/// Alta y listado de asignaciones de docentes a materias en cursos
/// (RF-NEG-02, RD-02, RD-03, RD-12). La lógica vive aquí, no en el controlador.
/// </summary>
public interface IAsignacionesServicio
{
    /// <summary>
    /// Asigna un docente con rol Estándar y cuenta activa a una materia de un curso.
    /// </summary>
    Task<AsignacionAdministracionResumen> CrearAsync(
        int usuarioId,
        int materiaId,
        int cursoId,
        int horasSemanales,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista las asignaciones, con filtros opcionales por curso y por docente.
    /// </summary>
    Task<IReadOnlyList<AsignacionAdministracionResumen>> ListarAsync(
        int? cursoId,
        int? usuarioId,
        CancellationToken cancellationToken = default);
}
