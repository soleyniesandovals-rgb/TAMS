namespace Core.ControlAcceso.Application.Interfaces;

/// <summary>Flujo de recuperación de contraseña por correo (RF-CA-09 a RF-CA-13).</summary>
public interface IRecuperacionContrasenaServicio
{
    /// <summary>
    /// Inicia la recuperación para el correo indicado. La respuesta es idéntica exista
    /// o no la cuenta, sin distinguir por mensaje ni por tiempo (RF-CA-09). Si la
    /// cuenta existe, emite un código de un solo uso y encola el correo (RF-CA-10).
    /// </summary>
    Task IniciarAsync(string correo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Establece la nueva contraseña a partir de un código válido. Un código
    /// inexistente, ya usado o vencido se rechaza sin cambiar la contraseña (RF-CA-11)
    /// y la contraseña nueva invalida las sesiones previas (RF-CA-12).
    /// </summary>
    Task ConfirmarAsync(string codigo, string nuevaContrasena, CancellationToken cancellationToken = default);

    /// <summary>
    /// Fuerza el restablecimiento de la cuenta indicada: emite un código de un solo
    /// uso y encola el correo, sin cambiar la contraseña directamente (RF-CA-13).
    /// Solo debe invocarse tras autorizar al Administrador.
    /// </summary>
    Task RestablecerAsync(int usuarioId, CancellationToken cancellationToken = default);
}
