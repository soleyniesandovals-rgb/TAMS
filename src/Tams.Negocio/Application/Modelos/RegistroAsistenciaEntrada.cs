namespace Tams.Negocio.Application.Modelos;

/// <summary>
/// Datos de entrada para guardar la asistencia de un estudiante en una fecha.
/// </summary>
public record RegistroAsistenciaEntrada(
    int EstudianteId,
    string Estado,
    string? Observacion);
