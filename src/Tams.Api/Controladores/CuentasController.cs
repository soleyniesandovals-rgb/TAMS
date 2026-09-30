using Core.ControlAcceso.Application.Interfaces;
using Core.ControlAcceso.Application.Modelos;
using Core.ControlAcceso.Domain.Excepciones;
using Microsoft.AspNetCore.Mvc;

namespace Tams.Api.Controladores;

/// <summary>
/// Registro y activación de cuentas (Práctica 1, sección 1.1).
/// RD-02: la lógica de negocio vive en los servicios de Application, no aquí.
/// RD-08: los mensajes de error nunca exponen trazas ni detalles internos.
/// </summary>
[ApiController]
[Route("api/cuentas")]
public class CuentasController(IRegistroCuentaServicio registro, IActivacionCuentaServicio activacion, IReenvioActivacionServicio reenvio, IAutenticacionServicio autenticacion) : ControllerBase
{
    /// <summary>Registra una cuenta; nacen inactivas y se activan por enlace (RF-CA-01/02/14/15).</summary>
    [HttpPost("registro")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegistroAsync(RegistroCuentaRequest peticion, CancellationToken ct)
    {
        try
        {
            await registro.RegistrarseAsync(peticion.Nombre, peticion.Correo, peticion.Contrasena, ct);
            return StatusCode(StatusCodes.Status201Created,
                new { mensaje = "Cuenta creada. Revisa tu correo para activarla." });
        }
        catch (ReglaNegocioExcepcion ex)
        {
            return RespuestaDeError(ex);
        }
    }

    /// <summary>
    /// Activa la cuenta con el enlace del correo (RF-CA-16). Es GET para que el enlace
    /// funcione directamente desde el navegador.
    /// </summary>
    [HttpGet("activar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ActivarAsync([FromQuery] string codigo, CancellationToken ct)
    {
        try
        {
            await activacion.ActivarAsync(codigo, ct);
            return Ok(new { mensaje = "Cuenta activada. Ya puedes iniciar sesión." });
        }
        catch (ReglaNegocioExcepcion ex)
        {
            return RespuestaDeError(ex);
        }
    }

    /// <summary>
    /// Reenvía el enlace de activación. La respuesta es idéntica exista o no el correo,
    /// por mensaje y por trabajo realizado (RF-CA-17).
    /// </summary>
    [HttpPost("reenviar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ReenviarAsync(ReenviarActivacionRequest peticion, CancellationToken ct)
    {
        const string respuesta = "Si el correo está registrado, recibirás un nuevo enlace de activación.";

        try
        {
            await reenvio.ReenviarAsync(peticion.Correo, ct);
            return Ok(new { mensaje = respuesta });
        }
        catch (ReglaNegocioExcepcion ex)
        {
            // Único caso distinguible: formato de correo inválido (RD-07). La
            // existencia de la cuenta nunca se delata (RF-CA-17).
            return RespuestaDeError(ex);
        }
    }

    /// <summary>Valida credenciales; rechaza las cuentas sin activar (RF-CA-15).</summary>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> IniciarSesionAsync(IniciarSesionRequest peticion, CancellationToken ct)
    {
        try
        {
            var resultado = await autenticacion.AutenticarAsync(peticion.Correo, peticion.Contrasena, ct);

            return resultado.Estado switch
            {
                EstadoAutenticacion.Exito when resultado.Usuario is not null => Ok(new
                {
                    nombre = resultado.Usuario.Nombre,
                    correo = resultado.Usuario.Correo,
                    rol = resultado.Usuario.Rol.ToString(),
                }),
                EstadoAutenticacion.CuentaInactiva => StatusCode(StatusCodes.Status403Forbidden,
                    new { mensaje = "La cuenta no está activa. Actívala con el enlace enviado a tu correo." }),
                _ => StatusCode(StatusCodes.Status401Unauthorized,
                    new { mensaje = "Correo o contraseña incorrectos." }),
            };
        }
        catch (ReglaNegocioExcepcion ex)
        {
            return RespuestaDeError(ex);
        }
    }

    private IActionResult RespuestaDeError(ReglaNegocioExcepcion ex)
        => StatusCode(ex.CodigoHttp, new { mensaje = ex.Message });

    public sealed record RegistroCuentaRequest(string Nombre, string Correo, string Contrasena);

    public sealed record ReenviarActivacionRequest(string Correo);

    public sealed record IniciarSesionRequest(string Correo, string Contrasena);
}