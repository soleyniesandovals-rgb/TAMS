using Tams.Negocio.Application.Modelos;

namespace Tams.Negocio.Application.Servicios;

/// <summary>
/// Alta y listado de cursos y de los estudiantes de cada curso
/// (RF-NEG-01, RF-NEG-02, RD-02, RD-06, RD-07, RD-08, RD-12).
/// La lógica vive aquí, no en el controlador.
/// </summary>
public interface ICursosServicio
{
    /// <summary>Crea un curso dentro de un año escolar.</summary>
    Task<CursoResumen> CrearCursoAsync(
        string grado,
        string seccion,
        bool esTecnico,
        string? tipoTecnico,
        int anioEscolarId,
        CancellationToken cancellationToken = default);

    /// <summary>Lista los cursos, opcionalmente filtrados por año escolar.</summary>
    Task<IReadOnlyList<CursoResumen>> ListarCursosAsync(
        int? anioEscolarId,
        CancellationToken cancellationToken = default);

    /// <summary>Crea un estudiante con su cédula normalizada en un curso existente.</summary>
    Task<EstudianteAdministracionResumen> CrearEstudianteAsync(
        int cursoId,
        string nombre,
        string apellido,
        string? cedula,
        CancellationToken cancellationToken = default);

    /// <summary>Lista los estudiantes de un curso, ordenados por apellido y nombre.</summary>
    Task<IReadOnlyList<EstudianteAdministracionResumen>> ListarEstudiantesAsync(
        int cursoId,
        CancellationToken cancellationToken = default);
}
