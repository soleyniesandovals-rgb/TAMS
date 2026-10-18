namespace Core.Permisos.Application.Modelos;

/// <summary>
/// DTO de respuesta para una solicitud de permiso (RF-GP-01, RF-GP-02, RF-GP-03).
/// Expone el estado como texto legible y no incluye las navegaciones a <c>Usuario</c>.
/// </summary>
public sealed record SolicitudPermisoResumen(
    int Id,
    int SolicitanteId,
    string TipoPermiso,
    string Estado,
    DateTime FechaCreacion,
    int? ResueltaPorId,
    DateTime? FechaResolucion,
    string? MotivoRechazo);
