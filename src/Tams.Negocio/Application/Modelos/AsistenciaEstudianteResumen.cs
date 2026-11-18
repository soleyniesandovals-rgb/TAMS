namespace Tams.Negocio.Application.Modelos;

/// <summary>
/// Fila del reporte de asistencia (RF-REP-02, C-21): una por estudiante y
/// asignación, con la tasa de asistencia del rango consultado.
/// No expone la cédula del estudiante (dato personal) ni el usuario docente.
/// </summary>
/// <param name="MateriaNombre">Nombre de la materia de la asignación.</param>
/// <param name="Grado">Grado del curso.</param>
/// <param name="Seccion">Sección del curso.</param>
/// <param name="Nombre">Nombre del estudiante.</param>
/// <param name="Apellido">Apellido del estudiante.</param>
/// <param name="Presentes">Registros en estado Presente.</param>
/// <param name="Tardanzas">Registros en estado Tardanza.</param>
/// <param name="Ausencias">Registros en estado Ausente.</param>
/// <param name="Excusas">Registros en estado Excusa.</param>
/// <param name="Total">Total de registros del estudiante en la asignación.</param>
/// <param name="PorcentajeAsistencia">(Presentes + Tardanzas) / Total × 100, redondeado a 2 decimales.</param>
public sealed record AsistenciaEstudianteResumen(
    string MateriaNombre,
    string Grado,
    string Seccion,
    string Nombre,
    string Apellido,
    int Presentes,
    int Tardanzas,
    int Ausencias,
    int Excusas,
    int Total,
    decimal PorcentajeAsistencia);
