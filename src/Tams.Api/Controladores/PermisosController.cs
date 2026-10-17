using System.IdentityModel.Tokens.Jwt;
using Core.ControlAcceso.Domain.Enums;
using Core.ControlAcceso.Domain.Excepciones;
using Core.Permisos.Application.Interfaces;
using Core.Permisos.Domain.Excepciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tams.Api.Seguridad;

namespace Tams.Api.Controladores;

/// <summary>
/// Gestión de solicitudes de permiso y verificación de permisos otorgados
/// (Segunda pieza del Core, RF-GP-01 a RF-GP-07).
/// RD-02: la lógica de negocio vive en <c>IGestionPermisosServicio</c>, no aquí.
/// RD-06: las operaciones de resolución exigen rol Administrador del lado del servidor.
/// RD-08: los mensajes de error son controlados, sin trazas ni detalles internos.
/// </summary>
[ApiController]
[Route("api/permisos")]
public class PermisosController(IGestionPermisosServicio permisos) : ControllerBase
{
    /// <summary>Crea una solicitud de permiso para el usuario autenticado (RF-GP-01).</summary>
    [Authorize]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CrearAsync(CrearSolicitudPermisoRequest peticion, CancellationToken ct)
    {
        if (!TryObtenerUsuarioId(out var usuarioId))
        {
            return Unauthorized();
        }

        try
        {
            var creada = await permisos.CrearAsync(usuarioId, peticion.TipoPermiso, ct);
            return StatusCode(StatusCodes.Status201Created, creada);
        }
        catch (ReglaNegocioExcepcion ex)
        {
            return RespuestaDeError(ex);
        }
    }

    /// <summary>Lista las solicitudes del propio usuario autenticado (RF-GP-02).</summary>
    [Authorize]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ListarPropiasAsync(CancellationToken ct)
    {
        if (!TryObtenerUsuarioId(out var usuarioId))
        {
            return Unauthorized();
        }

        return Ok(await permisos.ListarPropiasAsync(usuarioId, ct));
    }

    /// <summary>
    /// Lista todas las solicitudes pendientes; solo Administrador (RF-GP-03, RD-06).
    /// Un Estandar recibe 403 por el atributo, sin ejecutar el servicio.
    /// </summary>
    [RequiereRol(RolUsuario.Administrador)]
    [HttpGet("pendientes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListarPendientesAsync(CancellationToken ct)
        => Ok(await permisos.ListarPendientesAsync(ct));

    /// <summary>
    /// Aprueba una solicitud y otorga el permiso en la misma operación; solo Administrador
    /// (RF-GP-05, RF-GP-06, RF-GP-07, RD-06).
    /// </summary>
    [RequiereRol(RolUsuario.Administrador)]
    [HttpPost("{id:int}/aprobar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AprobarAsync(int id, CancellationToken ct)
    {
        if (!TryObtenerUsuarioId(out var administradorId))
        {
            return Unauthorized();
        }

        try
        {
            var resuelta = await permisos.AprobarAsync(id, administradorId, ct);
            return Ok(resuelta);
        }
        catch (ReglaNegocioExcepcion ex)
        {
            return RespuestaDeError(ex);
        }
        catch (TransicionNoPermitidaExcepcion ex)
        {
            // RD-04/RF-GP-06: transición no habilitada (p. ej. aprobar dos veces) => 409.
            return Conflict(new { mensaje = ex.Message });
        }
    }

    /// <summary>
    /// Rechaza una solicitud con un motivo obligatorio; solo Administrador
    /// (RF-GP-04, RF-GP-06, RF-GP-07, RD-06).
    /// </summary>
    [RequiereRol(RolUsuario.Administrador)]
    [HttpPost("{id:int}/rechazar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RechazarAsync(int id, RechazarSolicitudRequest peticion, CancellationToken ct)
    {
        if (!TryObtenerUsuarioId(out var administradorId))
        {
            return Unauthorized();
        }

        try
        {
            var resuelta = await permisos.RechazarAsync(id, administradorId, peticion.Motivo, ct);
            return Ok(resuelta);
        }
        catch (ReglaNegocioExcepcion ex)
        {
            return RespuestaDeError(ex);
        }
        catch (TransicionNoPermitidaExcepcion ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
    }

    /// <summary>
    /// Verifica si el usuario autenticado tiene otorgado el permiso indicado (RF-GP-05).
    /// 200 si lo tiene, 403 si no. No es un error de negocio: la falta de permiso es un
    /// resultado esperado, por eso se responde con <see cref="ControllerBase.Forbid()"/>.
    /// </summary>
    [Authorize]
    [HttpGet("acceso/{tipoPermiso}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> VerificarAccesoAsync(string tipoPermiso, CancellationToken ct)
    {
        if (!TryObtenerUsuarioId(out var usuarioId))
        {
            return Unauthorized();
        }

        var tienePermiso = await permisos.TienePermisoAsync(usuarioId, tipoPermiso, ct);

        return tienePermiso
            ? Ok(new { mensaje = "Acceso concedido." })
            : Forbid();
    }

    private bool TryObtenerUsuarioId(out int usuarioId)
        => int.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out usuarioId);

    private IActionResult RespuestaDeError(ReglaNegocioExcepcion ex)
        => StatusCode(ex.CodigoHttp, new { mensaje = ex.Message });

    public sealed record CrearSolicitudPermisoRequest(string TipoPermiso);

    public sealed record RechazarSolicitudRequest(string Motivo);
}
