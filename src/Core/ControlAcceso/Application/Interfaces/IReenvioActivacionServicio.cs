namespace Core.ControlAcceso.Application.Interfaces;

public interface IReenvioActivacionServicio
{
    /// <summary>
    /// Reenvía el enlace de activación para el correo indicado. La respuesta es
    /// idéntica exista o no la cuenta, sin distinguir por mensaje ni por tiempo
    /// (RF-CA-17). Invalida el token anterior y crea uno nuevo.
    /// </summary>
    Task ReenviarAsync(string correo, CancellationToken cancellationToken = default);
}