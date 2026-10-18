using Core.Permisos.Domain.Entidades;

namespace Core.Permisos.Application.Interfaces;

/// <summary>
/// Acceso a las solicitudes de permiso (RF-GP-01, RF-GP-02, RF-GP-03).
/// </summary>
public interface ISolicitudesPermisoRepositorio
{
    Task<SolicitudPermiso?> BuscarPorIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Solicitudes de un usuario, para que solo vea las propias (RF-GP-02).</summary>
    Task<IReadOnlyList<SolicitudPermiso>> ListarPorSolicitanteAsync(int solicitanteId, CancellationToken cancellationToken = default);

    /// <summary>Solicitudes aún sin resolver, para el Administrador (RF-GP-03).</summary>
    Task<IReadOnlyList<SolicitudPermiso>> ListarPendientesAsync(CancellationToken cancellationToken = default);

    Task AgregarAsync(SolicitudPermiso solicitud, CancellationToken cancellationToken = default);
}
