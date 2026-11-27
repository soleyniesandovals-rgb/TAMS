namespace Tams.Negocio.Application.Modelos;

/// <summary>
/// Lista de asistencia de una asignación docente en una fecha. Incluye a todos
/// los estudiantes del curso, ordenados por apellido y nombre.
/// </summary>
public record ListaAsistenciaResumen(
    int AsignacionId,
    string MateriaNombre,
    string Grado,
    string Seccion,
    DateOnly Fecha,
    bool Guardada,
    DateTime? EditadaEn,
    IReadOnlyList<RegistroAsistenciaResumen> Registros);
