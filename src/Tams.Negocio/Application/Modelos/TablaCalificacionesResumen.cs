namespace Tams.Negocio.Application.Modelos;

/// <summary>
/// Tabla completa de calificaciones de una asignación. Incluye las columnas
/// según la materia y a todos los estudiantes del curso, con o sin notas.
/// </summary>
public record TablaCalificacionesResumen(
    int AsignacionId,
    int MateriaId,
    string MateriaNombre,
    bool EsTecnica,
    string Grado,
    string Seccion,
    int AnioEscolarId,
    string AnioEscolarNombre,
    IReadOnlyList<ColumnaCalificacionResumen> Columnas,
    IReadOnlyList<EstudianteCalificacionesResumen> Estudiantes);
