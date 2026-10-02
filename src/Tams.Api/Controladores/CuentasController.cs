using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Core.ControlAcceso.Application.Interfaces;
using Core.ControlAcceso.Application.Modelos;
using Core.ControlAcceso.Domain.Enums;
using Core.ControlAcceso.Domain.Excepciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tams.Api.Seguridad;

namespace Tams.Api.Controladores;

/// <summary>
/// Registro, activación y sesión de cuentas, y administración de roles
/// (Práctica 1, secciones 1.1 a 1.3).
/// RD-02: la lógica de negocio vive en los servicios de Application, no aquí.
/// RD-06: las operaciones administrativas exigen rol Administrador en el servidor.
/// RD-08: los mensajes de error nunca exponen trazas ni detalles internos.
/// </summary>
[ApiController]
[Route("api/cuentas")]
public class CuentasController(
    IRegistroCuentaServicio registro,
    IActivacionCuentaServicio activacion,
    IReenvioActivacionServicio reenvio,
    IAutenticacionServicio autenticacion,
    ISesionServicio sesiones,
    IAdministracionCuentasServicio administracion) : ControllerBase
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

    /// <summary>Inicia sesión y emite el JWT de acceso (RF-CA-03/15/19).</summary>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status423Locked)]
    public async Task<IActionResult> IniciarSesionAsync(IniciarSesionRequest peticion, CancellationToken ct)
    {
        try
        {
            var resultado = await autenticacion.AutenticarAsync(peticion.Correo, peticion.Contrasena, ct);

            return resultado.Estado switch
            {
                EstadoAutenticacion.Exito => Ok(new
                {
                    token = resultado.Token,
                    expiraEn = resultado.ExpiraEn,
                }),
                EstadoAutenticacion.CuentaInactiva => StatusCode(StatusCodes.Status403Forbidden,
                    new { mensaje = "La cuenta no está activa. Actívala con el enlace enviado a tu correo." }),
                EstadoAutenticacion.CuentaBloqueada => StatusCode(StatusCodes.Status423Locked,
                    new
                    {
                        mensaje = $"La cuenta está bloqueada temporalmente por demasiados intentos fallidos. "
                                  + $"Inténtalo de nuevo a partir de las {resultado.BloqueadoHasta:HH\\:mm} UTC.",
                        bloqueadoHasta = resultado.BloqueadoHasta,
                    }),
                _ => StatusCode(StatusCodes.Status401Unauthorized,
                    new { mensaje = "Correo o contraseña incorrectos." }),
            };
        }
        catch (ReglaNegocioExcepcion ex)
        {
            return RespuestaDeError(ex);
        }
    }

    /// <summary>
    /// Devuelve los datos del usuario autenticado leyendo los claims del JWT (RF-CA-07).
    /// Sin sesión válida, el middleware responde 401 automáticamente.
    /// </summary>
    [Authorize]
    [HttpGet("yo")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult ObtenerUsuarioAutenticado()
    {
        if (!int.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var id))
        {
            return Unauthorized();
        }

        return Ok(new
        {
            id,
            correo = User.FindFirst(JwtRegisteredClaimNames.Email)?.Value,
            nombre = User.FindFirst(ClaimTypes.Name)?.Value,
            rol = User.FindFirst(ClaimTypes.Role)?.Value,
        });
    }

    /// <summary>
    /// Invalida la sesión actual (y todas las anteriores) incrementando SesionVersion
    /// (RF-CA-18): el validador Bearer rechaza el JWT actual y cualquier otro anterior.
    /// </summary>
    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CerrarSesionAsync(CancellationToken ct)
    {
        if (!int.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var id))
        {
            return Unauthorized();
        }

        await sesiones.CerrarSesionAsync(id, ct);
        return NoContent();
    }

    /// <summary>
    /// Lista todos los usuarios con su rol y estado (activo/inactivo), solo para
    /// Administrador (RF-CA-21, RD-06). Devuelve un DTO sin ContraseñaHash ni tokens.
    /// </summary>
    [RequiereRol(RolUsuario.Administrador)]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListarUsuariosAsync(CancellationToken ct)
        => Ok(await administracion.ListarAsync(ct));

    /// <summary>
    /// Cambia el rol de otro usuario, solo para Administrador (RF-CA-08, RD-06).
    /// Un Administrador no puede cambiar su propio rol: así el sistema nunca queda
    /// sin administradores.
    /// </summary>
    [RequiereRol(RolUsuario.Administrador)]
    [HttpPut("{id:int}/rol")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CambiarRolAsync(int id, CambiarRolRequest peticion, CancellationToken ct)
    {
        if (!TryObtenerUsuarioId(out var administradorId))
        {
            return Unauthorized();
        }

        try
        {
            // Se acepta el nombre del rol con cualquier combinación de mayúsculas,
            // pero solo los nombres válidos (no valores numéricos).
            if (!Enum.TryParse<RolUsuario>(peticion.Rol, ignoreCase: true, out var rol)
                || !string.Equals(rol.ToString(), peticion.Rol?.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                throw new ReglaNegocioExcepcion("El rol debe ser 'Administrador' o 'Estandar'.");
            }

            await administracion.CambiarRolAsync(id, rol, administradorId, ct);
            return NoContent();
        }
        catch (ReglaNegocioExcepcion ex)
        {
            return RespuestaDeError(ex);
        }
    }

    /// <summary>
    /// Desactiva una cuenta, solo para Administrador (RF-CA-20, RD-06). El usuario no
    /// podrá iniciar sesión y sus sesiones abiertas dejan de ser válidas.
    /// </summary>
    [RequiereRol(RolUsuario.Administrador)]
    [HttpPost("{id:int}/desactivar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DesactivarAsync(int id, CancellationToken ct)
    {
        if (!TryObtenerUsuarioId(out var administradorId))
        {
            return Unauthorized();
        }

        try
        {
            await administracion.DesactivarAsync(id, administradorId, ct);
            return NoContent();
        }
        catch (ReglaNegocioExcepcion ex)
        {
            return RespuestaDeError(ex);
        }
    }

    /// <summary>
    /// Reactiva una cuenta, solo para Administrador (RF-CA-20, RD-06). Limpia cualquier
    /// bloqueo por intentos fallidos para que la cuenta pueda volver a iniciar sesión.
    /// </summary>
    [RequiereRol(RolUsuario.Administrador)]
    [HttpPost("{id:int}/reactivar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReactivarAsync(int id, CancellationToken ct)
    {
        try
        {
            await administracion.ReactivarAsync(id, ct);
            return NoContent();
        }
        catch (ReglaNegocioExcepcion ex)
        {
            return RespuestaDeError(ex);
        }
    }

    private bool TryObtenerUsuarioId(out int usuarioId)
        => int.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out usuarioId);

    private IActionResult RespuestaDeError(ReglaNegocioExcepcion ex)
        => StatusCode(ex.CodigoHttp, new { mensaje = ex.Message });

    public sealed record RegistroCuentaRequest(string Nombre, string Correo, string Contrasena);

    public sealed record ReenviarActivacionRequest(string Correo);

    public sealed record IniciarSesionRequest(string Correo, string Contrasena);

    public sealed record CambiarRolRequest(string Rol);
}