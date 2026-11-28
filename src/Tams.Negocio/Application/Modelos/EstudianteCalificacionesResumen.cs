namespace Tams.Negocio.Application.Modelos;

/// <summary>
/// Fila de la tabla de calificaciones: estudiante del curso con sus notas y su
/// acumulado calculado en el servidor. Sin cédula.
/// </summary>
public record EstudianteCalificacionesResumen(
    int EstudianteId,
    string Nombre,
    string Apellido,
    IReadOnlyList<NotaCalificacionResumen> Notas,
    decimal? Acumulado);
