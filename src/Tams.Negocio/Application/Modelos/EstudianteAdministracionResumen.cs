namespace Tams.Negocio.Application.Modelos;

/// <summary>
/// Datos de un estudiante para el Administrador. Incluye la cédula, un dato
/// personal de un menor que no se expone en los listados del docente.
/// </summary>
public record EstudianteAdministracionResumen(
    int Id,
    string Nombre,
    string Apellido,
    string? Cedula,
    int CursoId);
