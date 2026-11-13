namespace Tams.Negocio.Application.Modelos;

/// <summary>
/// Asignación docente con los datos de materia, curso y año escolar para consultas del docente.
/// </summary>
public record AsignacionDocenteResumen(
    int Id,
    int MateriaId,
    string MateriaNombre,
    bool EsTecnica,
    int? CantidadRa,
    int CursoId,
    string Grado,
    string Seccion,
    int AnioEscolarId,
    string AnioEscolarNombre,
    int HorasSemanales);
