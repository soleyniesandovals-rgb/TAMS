namespace Core.ControlAcceso.Application.Interfaces;

public interface IActivacionCuentaServicio
{
    /// <summary>
    /// Activa la cuenta del usuario ligado al token. Un token ya usado, vencido o
    /// inexistente se rechaza con mensaje controlado y nada cambia (RF-CA-16).
    /// Devuelve sin valor; la cuenta queda <c>Activo = true</c> y el token <c>Usado = true</c>.
    /// </summary>
    Task ActivarAsync(string codigo, CancellationToken cancellationToken = default);
}