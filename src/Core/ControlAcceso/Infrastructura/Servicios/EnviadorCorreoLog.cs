using Core.ControlAcceso.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Core.ControlAcceso.Infrastructura.Servicios;

/// <summary>
/// Enviador de correo para entornos sin SMTP (RD-10). Escribe en el log el
/// destinatario, el asunto y el cuerpo (que incluye el enlace) y da el correo
/// por enviado. Se usa cuando TAMS_SMTP_HOST no está definida o está vacía.
/// </summary>
public sealed class EnviadorCorreoLog(ILogger<EnviadorCorreoLog> logger) : IEnviadorCorreo
{
    public Task EnviarAsync(
        string destinatario,
        string asunto,
        string cuerpo,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Correo simulado (modo sin SMTP). Para: {Destinatario}. Asunto: {Asunto}. Cuerpo: {Cuerpo}",
            destinatario,
            asunto,
            cuerpo);

        return Task.CompletedTask;
    }
}
