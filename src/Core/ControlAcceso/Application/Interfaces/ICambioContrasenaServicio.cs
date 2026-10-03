namespace Core.ControlAcceso.Application.Interfaces;

/// <summary>Cambio de contraseña por parte de un usuario autenticado (RF-CA-22).</summary>
public interface ICambioContrasenaServicio
{
    /// <summary>
    /// Cambia la contraseña del usuario si la actual coincide. Aplica la política
    /// (RF-CA-14) y sube la versión de sesión para invalidar sesiones previas (RF-CA-12).
    /// </summary>
    Task CambiarAsync(
        int usuarioId,
        string contrasenaActual,
        string contrasenaNueva,
        CancellationToken cancellationToken = default);
}
