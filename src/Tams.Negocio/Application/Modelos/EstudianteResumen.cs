namespace Tams.Negocio.Application.Modelos;

/// <summary>
/// Datos básicos de un estudiante para listados de consulta.
/// </summary>
public record EstudianteResumen(
    int Id,
    string Nombre,
    string Apellido,
    int CursoId);
