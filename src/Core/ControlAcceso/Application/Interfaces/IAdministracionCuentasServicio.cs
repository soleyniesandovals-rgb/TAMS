using Core.ControlAcceso.Application.Modelos;

namespace Core.ControlAcceso.Application.Interfaces;

/// <summary>
/// Operaciones de administración de cuentas reservadas al rol Administrador
/// (RF-CA-08, RF-CA-20, RF-CA-21).
/// </summary>
public interface IAdministracionCuentasServicio
{
    /// <summary>Lista todos los usuarios con su rol y estado, sin datos sensibles (RF-CA-21).</summary>
    Task<IReadOnlyList<UsuarioResumen>> ListarAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Cambia el rol de otro usuario (RF-CA-08). Recibe el nombre del rol tal como llega
    /// del exterior y lo valida en el servicio (RD-02, RD-07): si no es un rol válido,
    /// lanza <c>ReglaNegocioExcepcion</c> y el controlador solo la traduce a respuesta.
    /// Nadie puede cambiar su propio rol, ni siquiera un Administrador: así siempre queda
    /// al menos un administrador.
    /// </summary>
    Task CambiarRolAsync(int usuarioId, string nuevoRol, int administradorId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Desactiva una cuenta e invalida sus sesiones abiertas (RF-CA-20). Un
    /// Administrador no puede desactivarse a sí mismo.
    /// </summary>
    Task DesactivarAsync(int usuarioId, int administradorId, CancellationToken cancellationToken = default);

    /// <summary>Reactiva una cuenta y limpia cualquier bloqueo por intentos fallidos (RF-CA-20).</summary>
    Task ReactivarAsync(int usuarioId, CancellationToken cancellationToken = default);
}