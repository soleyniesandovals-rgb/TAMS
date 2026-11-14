namespace Tams.Negocio.Application.Modelos;

/// <summary>
/// Asignación docente con los datos de materia, curso y año escolar para el
/// Administrador. El nombre y el correo del docente pueden ser null si el usuario
/// referenciado ya no existe en el Core.
/// </summary>
public record AsignacionAdministracionResumen(
    int Id,
    int UsuarioId,
    string? DocenteNombre,
    string? DocenteCorreo,
    int MateriaId,
    string MateriaNombre,
    int CursoId,
    string Grado,
    string Seccion,
    int AnioEscolarId,
    string AnioEscolarNombre,
    int HorasSemanales);
