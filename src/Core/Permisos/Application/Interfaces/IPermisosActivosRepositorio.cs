using Core.Permisos.Domain.Entidades;

namespace Core.Permisos.Application.Interfaces;

/// <summary>
/// Acceso a los permisos otorgados a los usuarios (RF-GP-05).
/// </summary>
public interface IPermisosActivosRepositorio
{
    /// <summary>
    /// Indica si el usuario ya tiene otorgado ese tipo de permiso. Evita duplicados
    /// aunque se aprueben dos solicitudes del mismo tipo (RF-GP-05).
    /// </summary>
    Task<bool> ExistePermisoAsync(int usuarioId, string tipoPermiso, CancellationToken cancellationToken = default);

    Task AgregarAsync(PermisoActivo permiso, CancellationToken cancellationToken = default);
}
