namespace Core.Auditoria.Domain.Enums;

/// <summary>
/// Acciones auditables del historial (CA-01, RF-AUD-01). Los nombres son exactamente
/// los publicados en docs/criterios-auditoria.md.
/// </summary>
public enum AccionAuditoria
{
    AprobarSolicitud,
    RechazarSolicitud,
    SubirDocumento,
    EliminarDocumento,
    CambiarRol,
    DesactivarUsuario,
    ReactivarUsuario,
    RestablecerContrasena,
    RecuperarContrasena,
    CambiarContrasena,
}
