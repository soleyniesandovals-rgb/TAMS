using Core.Permisos.Application.Modelos;

namespace Core.Permisos.Application.Interfaces;

/// <summary>
/// Gestión de solicitudes de permiso y verificación de permisos otorgados
/// (RF-GP-01 a RF-GP-07). La lógica de negocio vive aquí, no en el controlador (RD-02).
/// </summary>
public interface IGestionPermisosServicio
{
    /// <summary>Crea una solicitud en estado Pendiente para el usuario (RF-GP-01).</summary>
    Task<SolicitudPermisoResumen> CrearAsync(int solicitanteId, string tipoPermiso, CancellationToken cancellationToken = default);

    /// <summary>Lista únicamente las solicitudes del propio usuario (RF-GP-02).</summary>
    Task<IReadOnlyList<SolicitudPermisoResumen>> ListarPropiasAsync(int usuarioId, CancellationToken cancellationToken = default);

    /// <summary>Lista todas las solicitudes pendientes; operación exclusiva del Administrador (RF-GP-03).</summary>
    Task<IReadOnlyList<SolicitudPermisoResumen>> ListarPendientesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Aprueba una solicitud y, en la misma operación, otorga el permiso (RF-GP-05,
    /// RF-GP-06, RF-GP-07). Un Administrador no puede resolver su propia solicitud.
    /// </summary>
    Task<SolicitudPermisoResumen> AprobarAsync(int solicitudId, int administradorId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Rechaza una solicitud con un motivo obligatorio (RF-GP-04, RF-GP-06, RF-GP-07).
    /// Un Administrador no puede resolver su propia solicitud.
    /// </summary>
    Task<SolicitudPermisoResumen> RechazarAsync(int solicitudId, int administradorId, string motivo, CancellationToken cancellationToken = default);

    /// <summary>Indica si el usuario ya tiene otorgado ese tipo de permiso (RF-GP-05).</summary>
    Task<bool> TienePermisoAsync(int usuarioId, string tipoPermiso, CancellationToken cancellationToken = default);
}
