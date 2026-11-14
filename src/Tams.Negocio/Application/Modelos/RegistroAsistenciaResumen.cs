namespace Tams.Negocio.Application.Modelos;

/// <summary>
/// Estado de asistencia de un estudiante para una lista. Si la lista todavía no se
/// ha guardado, <see cref="Estado"/> y <see cref="Observacion"/> son null.
/// </summary>
public record RegistroAsistenciaResumen(
    int EstudianteId,
    string Nombre,
    string Apellido,
    string? Estado,
    string? Observacion);
