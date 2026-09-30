using System.Net.Mail;
using Core.ControlAcceso.Application.Interfaces;
using Core.ControlAcceso.Application.Opciones;
using Core.ControlAcceso.Domain.Entidades;
using Core.ControlAcceso.Domain.Enums;
using Core.ControlAcceso.Domain.Excepciones;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Core.ControlAcceso.Application.Servicios;

public class RegistroCuentaServicio(
    ICoreUnidadDeTrabajo unidad,
    IContrasenaHasher hasher,
    ITokenGenerador generador,
    TimeProvider timeProvider,
    IOptions<OpcionesActivacionCuenta> opciones) : IRegistroCuentaServicio
{
    public async Task<Usuario> RegistrarseAsync(
        string nombre,
        string correo,
        string contrasena,
        CancellationToken cancellationToken = default)
    {
        // RD-07: toda entrada externa se valida antes de usarse.
        ValidarEntrada(nombre, correo, contrasena);

        var correoNormalizado = NormalizarCorreo(correo);

        // RF-CA-01: el segundo registro con el mismo correo se rechaza con mensaje controlado.
        if (await unidad.Usuarios.BuscarPorCorreoAsync(correoNormalizado, cancellationToken) is not null)
        {
            throw new ReglaNegocioExcepcion("Ya existe una cuenta registrada con ese correo.", 409);
        }

        var usuario = new Usuario
        {
            Nombre = nombre.Trim(),
            Correo = correoNormalizado,
            // RF-CA-02: nunca se guarda la contraseña en texto plano, solo su hash BCrypt.
            ContraseñaHash = hasher.Hash(contrasena),
            Rol = RolUsuario.Estandar,
            // RF-CA-15: la cuenta nace inactiva y se activa con el enlace.
            Activo = false,
        };

        var token = CrearToken();
        usuario.TokensActivacion.Add(token);

        var correoEnCola = CrearCorreoActivacion(usuario, token.Codigo);

        await unidad.Usuarios.AgregarAsync(usuario, cancellationToken);
        await unidad.CorreosEnCola.AgregarAsync(correoEnCola, cancellationToken);

        try
        {
            await unidad.GuardarCambiosAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (EsViolacionDeUnicidad(ex))
        {
            // Carrera: otro registro con el mismo correo se guardó milisegundos antes.
            throw new ReglaNegocioExcepcion("Ya existe una cuenta registrada con ese correo.", 409);
        }

        return usuario;
    }

    /// <summary>
    /// RF-CA-14: la política exige mínimo 8 caracteres con letras y números.
    /// RD-07/RD-08: los mensajes son controlados, sin detalles internos.
    /// </summary>
    private static void ValidarEntrada(string nombre, string correo, string contrasena)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ReglaNegocioExcepcion("El nombre es obligatorio.");
        }

        if (nombre.Trim().Length > 150)
        {
            throw new ReglaNegocioExcepcion("El nombre no puede superar los 150 caracteres.");
        }

        if (!EsCorreoValido(correo))
        {
            throw new ReglaNegocioExcepcion("El correo no es válido.");
        }

        if (contrasena.Length < 8
            || !contrasena.Any(char.IsLetter)
            || !contrasena.Any(char.IsDigit))
        {
            throw new ReglaNegocioExcepcion(
                "La contraseña debe tener al menos 8 caracteres e incluir letras y números.");
        }
    }

    private static bool EsCorreoValido(string correo)
        => MailAddress.TryCreate(correo, out var direccion) && direccion.Address == correo;

    private static string NormalizarCorreo(string correo)
        => correo.Trim().ToLowerInvariant();

    private TokenActivacion CrearToken()
    {
        var ahoraUtc = timeProvider.GetUtcNow().UtcDateTime;

        return new TokenActivacion
        {
            Codigo = generador.Generar(),
            FechaEmision = ahoraUtc,
            FechaVencimiento = ahoraUtc.AddHours(opciones.Value.HorasVencimientoToken),
            Usado = false,
        };
    }

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

    private static bool EsViolacionDeUnicidad(DbUpdateException ex)
        => ex.InnerException is Microsoft.Data.SqlClient.SqlException sql && sql.Number is 2601 or 2627;
}