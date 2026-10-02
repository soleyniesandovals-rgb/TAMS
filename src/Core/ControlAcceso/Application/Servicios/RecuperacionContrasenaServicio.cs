using System.Net.Mail;
using Core.ControlAcceso.Application.Interfaces;
using Core.ControlAcceso.Application.Opciones;
using Core.ControlAcceso.Application.Reglas;
using Core.ControlAcceso.Domain.Entidades;
using Core.ControlAcceso.Domain.Enums;
using Core.ControlAcceso.Domain.Excepciones;
using Microsoft.Extensions.Options;

namespace Core.ControlAcceso.Application.Servicios;

public class RecuperacionContrasenaServicio(
    ICoreUnidadDeTrabajo unidad,
    ITokenGenerador generador,
    IContrasenaHasher hasher,
    TimeProvider timeProvider,
    IOptions<OpcionesRecuperacionContrasena> opciones) : IRecuperacionContrasenaServicio
{
    public async Task IniciarAsync(string correo, CancellationToken cancellationToken = default)
    {
        // RD-07: la entrada se valida antes de usarse. Solo el formato del correo puede
        // delimitarse; de la existencia de la cuenta no se dice nada (RF-CA-09).
        if (!EsCorreoValido(correo))
        {
            throw new ReglaNegocioExcepcion("El correo no es válido.");
        }

        var correoNormalizado = correo.Trim().ToLowerInvariant();

        // RF-CA-09: la respuesta debe ser idéntica exista o no el correo. El código se
        // genera ANTES de consultar la cuenta, de modo que ambos caminos pagan el mismo
        // costo criptográfico (mismo enfoque anti-enumeración por tiempo que RF-CA-17).
        var codigo = generador.Generar();

        var usuario = await unidad.Usuarios.BuscarPorCorreoAsync(correoNormalizado, cancellationToken);
        if (usuario is null)
        {
            // Cuenta inexistente: no se persiste nada, pero el trabajo ya se realizó.
            return;
        }

        await EmitirCodigoAsync(usuario, codigo, cancellationToken);
    }

    public async Task ConfirmarAsync(
        string codigo,
        string nuevaContrasena,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            throw new ReglaNegocioExcepcion("El código de recuperación no es válido.");
        }

        // RF-CA-14: se exige la política de contraseñas antes de tocar la base.
        PoliticaContrasena.Exigir(nuevaContrasena);

        var registro = await unidad.CodigosRecuperacion.BuscarPorCodigoAsync(codigo, cancellationToken);

        // RF-CA-11: código inexistente, ya usado o vencido se rechaza y la contraseña
        // NO cambia (los rechazos ocurren antes de mutar el usuario).
        if (registro is null)
        {
            throw new ReglaNegocioExcepcion("El código de recuperación no es válido.");
        }

        if (registro.Usado)
        {
            throw new ReglaNegocioExcepcion("El código de recuperación ya fue utilizado.");
        }

        if (registro.FechaVencimiento < timeProvider.GetUtcNow().UtcDateTime)
        {
            throw new ReglaNegocioExcepcion("El código de recuperación ha vencido.");
        }

        registro.Usado = true;
        AplicarNuevaContrasena(registro.Usuario, nuevaContrasena);

        await unidad.GuardarCambiosAsync(cancellationToken);
    }

    public async Task RestablecerAsync(int usuarioId, CancellationToken cancellationToken = default)
    {
        var usuario = await unidad.Usuarios.BuscarPorIdAsync(usuarioId, cancellationToken)
            ?? throw new ReglaNegocioExcepcion("El usuario no existe.", 404);

        // RF-CA-12 (defensa en profundidad): un restablecimiento forzado es un evento de
        // seguridad, así que se invalidan de inmediato las sesiones abiertas del afectado.
        usuario.SesionVersion++;

        // RF-CA-13: NO se cambia la contraseña; se emite un código para que el propio
        // usuario defina una nueva a través del flujo de recuperación.
        var codigo = generador.Generar();
        await EmitirCodigoAsync(usuario, codigo, cancellationToken);
    }

    /// <summary>
    /// Emite un código de recuperación de un solo uso (invalida los anteriores) y encola
    /// el correo (RF-CA-10, RF-CA-13). El envío real lo hace el procesador de la cola:
    /// aquí nunca se envía directo.
    /// </summary>
    private async Task EmitirCodigoAsync(Usuario usuario, string codigo, CancellationToken cancellationToken)
    {
        var ahoraUtc = timeProvider.GetUtcNow().UtcDateTime;

        await unidad.CodigosRecuperacion.MarcarUsadosDelUsuarioAsync(usuario.Id, cancellationToken);

        var registro = new CodigoRecuperacion
        {
            UsuarioId = usuario.Id,
            Codigo = codigo,
            FechaEmision = ahoraUtc,
            FechaVencimiento = ahoraUtc.AddHours(opciones.Value.HorasVencimientoCodigo),
            Usado = false,
        };

        await unidad.CodigosRecuperacion.AgregarAsync(registro, cancellationToken);
        await unidad.CorreosEnCola.AgregarAsync(CrearCorreoRecuperacion(usuario, codigo), cancellationToken);

        await unidad.GuardarCambiosAsync(cancellationToken);
    }

    private void AplicarNuevaContrasena(Usuario usuario, string nuevaContrasena)
    {
        // RF-CA-02: solo se guarda el hash BCrypt, nunca la contraseña en texto plano.
        usuario.ContraseñaHash = hasher.Hash(nuevaContrasena);

        // RF-CA-12: al cambiar la contraseña se invalidan las sesiones previas.
        usuario.SesionVersion++;

        // La recuperación prueba el control del correo: se limpia cualquier bloqueo por
        // intentos fallidos para que la nueva contraseña sirva de inmediato (RF-CA-19).
        usuario.IntentosFallidosConsecutivos = 0;
        usuario.BloqueadoHasta = null;
    }

    private CorreoEnCola CrearCorreoRecuperacion(Usuario usuario, string codigo)
    {
        var enlace = opciones.Value.UrlRecuperacionPlantilla.Replace("{codigo}", codigo);
        var horas = opciones.Value.HorasVencimientoCodigo;

        return new CorreoEnCola
        {
            Destinatario = usuario.Correo,
            Asunto = "Recupera tu contraseña en TAMS",
            Cuerpo = $"Hola {usuario.Nombre}, tu código para restablecer la contraseña es: {codigo}. "
                     + $"Puedes usarlo aquí: {enlace}. Vence en {horas} hora(s).",
            Estado = EstadoCorreo.Pendiente,
        };
    }

    private static bool EsCorreoValido(string correo)
        => MailAddress.TryCreate(correo, out var direccion) && direccion.Address == correo;
}
