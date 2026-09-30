using System.Net.Mail;
using Core.ControlAcceso.Application.Interfaces;
using Core.ControlAcceso.Application.Opciones;
using Core.ControlAcceso.Domain.Entidades;
using Core.ControlAcceso.Domain.Enums;
using Core.ControlAcceso.Domain.Excepciones;
using Microsoft.Extensions.Options;

namespace Core.ControlAcceso.Application.Servicios;

public class ReenvioActivacionServicio(
    ICoreUnidadDeTrabajo unidad,
    ITokenGenerador generador,
    TimeProvider timeProvider,
    IOptions<OpcionesActivacionCuenta> opciones) : IReenvioActivacionServicio
{
    public async Task ReenviarAsync(string correo, CancellationToken cancellationToken = default)
    {
        // RD-07: la entrada se valida antes de usarse. Solo el formato del correo
        // puede delimitarse; de la existencia de la cuenta no se dice nada (RF-CA-17).
        if (!EsCorreoValido(correo))
        {
            throw new ReglaNegocioExcepcion("El correo no es válido.");
        }

        var correoNormalizado = correo.Trim().ToLowerInvariant();

        // RF-CA-17: la respuesta debe ser idéntica exista o no el correo. El token se
        // genera ANTES de consultar la cuenta, de modo que ambos caminos pagan el mismo
        // costo criptográfico y el mensaje devuelto es siempre el mismo.
        var codigo = generador.Generar();

        var usuario = await unidad.Usuarios.BuscarPorCorreoAsync(correoNormalizado, cancellationToken);
        if (usuario is null)
        {
            // Cuenta inexistente: no se persiste nada, pero el trabajo ya se realizó.
            return;
        }

        var ahoraUtc = timeProvider.GetUtcNow().UtcDateTime;

        // Invalida el token anterior marcándolo como usado y crea uno nuevo.
        await unidad.TokenActivaciones.MarcarUsadosDelUsuarioAsync(usuario.Id, cancellationToken);

        var token = new TokenActivacion
        {
            UsuarioId = usuario.Id,
            Codigo = codigo,
            FechaEmision = ahoraUtc,
            FechaVencimiento = ahoraUtc.AddHours(opciones.Value.HorasVencimientoToken),
            Usado = false,
        };

        await unidad.TokenActivaciones.AgregarAsync(token, cancellationToken);
        await unidad.CorreosEnCola.AgregarAsync(CrearCorreoActivacion(usuario, codigo), cancellationToken);

        await unidad.GuardarCambiosAsync(cancellationToken);
    }

    private static bool EsCorreoValido(string correo)
        => MailAddress.TryCreate(correo, out var direccion) && direccion.Address == correo;

    private CorreoEnCola CrearCorreoActivacion(Usuario usuario, string codigo)
    {
        var enlace = opciones.Value.UrlActivacionPlantilla.Replace("{codigo}", codigo);

        return new CorreoEnCola
        {
            Destinatario = usuario.Correo,
            Asunto = "Activa tu cuenta en TAMS",
            Cuerpo = $"Hola {usuario.Nombre}, para activar tu cuenta abre este enlace: {enlace}",
            Estado = EstadoCorreo.Pendiente,
        };
    }
}