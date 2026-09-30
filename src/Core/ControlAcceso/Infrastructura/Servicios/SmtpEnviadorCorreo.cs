using Core.ControlAcceso.Application.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Core.ControlAcceso.Infrastructura.Servicios;

public class SmtpEnviadorCorreo(OpcionesSmtp opciones) : IEnviadorCorreo
{
    public async Task EnviarAsync(string destinatario, string asunto, string cuerpo, CancellationToken cancellationToken = default)
    {
        using var cliente = new SmtpClient();

        // StartTlsWhenAvailable: sube a TLS si el servidor lo ofrece y permite probar
        // contra servidores SMTP de desarrollo sin TLS (versión mínima).
        await cliente.ConnectAsync(opciones.Host, opciones.Puerto, SecureSocketOptions.StartTlsWhenAvailable, cancellationToken);
        await cliente.AuthenticateAsync(opciones.Usuario, opciones.Contrasena, cancellationToken);

        var mensaje = new MimeMessage();
        mensaje.From.Add(MailboxAddress.Parse(opciones.Remitente));
        mensaje.To.Add(MailboxAddress.Parse(destinatario));
        mensaje.Subject = asunto;
        mensaje.Body = new TextPart("plain") { Text = cuerpo };

        await cliente.SendAsync(mensaje, cancellationToken);
        await cliente.DisconnectAsync(true, cancellationToken);
    }
}