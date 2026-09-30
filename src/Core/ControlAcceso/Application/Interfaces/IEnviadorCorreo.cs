namespace Core.ControlAcceso.Application.Interfaces;

/// <summary>
/// Abstracción del envío de correo (RD-12). La implementación con MailKit vive en
/// Infrastructura; Application solo depende de este contrato.
/// </summary>
public interface IEnviadorCorreo
{
    Task EnviarAsync(string destinatario, string asunto, string cuerpo, CancellationToken cancellationToken = default);
}