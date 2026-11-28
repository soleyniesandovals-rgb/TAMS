using System.IdentityModel.Tokens.Jwt;
using Core.ControlAcceso.Domain.Enums;
using Core.ControlAcceso.Domain.Excepciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Tams.Api.Seguridad;
using Tams.Negocio.Application.Modelos;
using Tams.Negocio.Application.Servicios;

namespace Tams.Api.Controladores;

/// <summary>
/// Eventos del calendario y próximos eventos (RF-NEG, RD-02, RD-06, RD-08).
/// Cualquier usuario con sesión lee; solo un Administrador crea y borra. El controlador
/// solo recibe, llama al servicio y traduce el resultado a HTTP.
/// </summary>
[ApiController]
[Authorize]
[RequiereRol(RolUsuario.Estandar, RolUsuario.Administrador)]
[Route("api/eventos")]
public class EventosController(IEventosCalendarioServicio eventos) : ControllerBase
{
    /// <summary>Lista los eventos de un año escolar.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<EventoCalendarioResumen>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListarAsync(
        [FromQuery] int? anioEscolarId,
        CancellationToken ct)
    {
        if (anioEscolarId is null)
        {
            return BadRequest(new { mensaje = "Indica el año escolar." });
        }

        try
        {
            return Ok(await eventos.ListarAsync(anioEscolarId.Value, ct));
        }
        catch (ReglaNegocioExcepcion ex)
        {
            return StatusCode(ex.CodigoHttp, new { mensaje = ex.Message });
        }
    }

    /// <summary>Lista eventos propios y feriados desde hoy hasta el fin del año activo.</summary>
    [HttpGet("proximos")]
    [ProducesResponseType(typeof(ProximosEventosResumen), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ProximosAsync(CancellationToken ct)
    {
        try
        {
            return Ok(await eventos.ProximosAsync(ct));
        }
        catch (ReglaNegocioExcepcion ex)
        {
            return StatusCode(ex.CodigoHttp, new { mensaje = ex.Message });
        }
    }

    /// <summary>Crea un evento propio. El usuario se toma del token, no del cuerpo.</summary>
    [HttpPost]
    [RequiereRol(RolUsuario.Administrador)]
    [ProducesResponseType(typeof(EventoCalendarioResumen), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CrearAsync(
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] CrearEventoRequest? peticion,
        CancellationToken ct)
    {
        if (!TryObtenerUsuarioId(out var usuarioId))
        {
            return Unauthorized();
        }

        if (peticion is null)
        {
            return BadRequest(new { mensaje = "El cuerpo de la petición es obligatorio." });
        }

        try
        {
            var creado = await eventos.CrearAsync(
                peticion.AnioEscolarId,
                peticion.Fecha,
                peticion.Descripcion ?? string.Empty,
                usuarioId,
                ct);

            return StatusCode(StatusCodes.Status201Created, creado);
        }
        catch (ReglaNegocioExcepcion ex)
        {
            return StatusCode(ex.CodigoHttp, new { mensaje = ex.Message });
        }
    }

    /// <summary>Borra un evento propio. Los feriados no se guardan ni se borran.</summary>
    [HttpDelete("{id:int}")]
    [RequiereRol(RolUsuario.Administrador)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BorrarAsync(int id, CancellationToken ct)
    {
        try
        {
            await eventos.BorrarAsync(id, ct);
            return NoContent();
        }
        catch (ReglaNegocioExcepcion ex)
        {
            return StatusCode(ex.CodigoHttp, new { mensaje = ex.Message });
        }
    }

    private bool TryObtenerUsuarioId(out int usuarioId)
        => int.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out usuarioId);
}

/// <summary>Datos del alta de un evento del calendario.</summary>
/// <param name="AnioEscolarId">Año escolar al que pertenece el evento.</param>
/// <param name="Fecha">Fecha del evento, en formato aaaa-mm-dd.</param>
/// <param name="Descripcion">Texto de «Qué hay ese día», de 1 a 200 caracteres.</param>
public sealed record CrearEventoRequest(int AnioEscolarId, DateOnly Fecha, string? Descripcion);
