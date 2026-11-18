using System.IdentityModel.Tokens.Jwt;
using Core.ControlAcceso.Domain.Excepciones;
using Core.Notificaciones.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Tams.Api.Controladores;

/// <summary>
/// Bandeja y preferencias de notificaciones del usuario autenticado
/// (RF-NOT-04, RF-NOT-05, RF-NOT-07, RD-06, RD-08, RD-11).
/// RD-02: el controlador solo recibe, llama al servicio y traduce el resultado a HTTP.
/// </summary>
[ApiController]
[Route("api/notificaciones")]
[Authorize]
public class NotificacionesController(INotificacionesServicio notificaciones) : ControllerBase
{
    /// <summary>Devuelve las notificaciones propias, de la más reciente a la más antigua.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ListarPropiasAsync(CancellationToken ct)
    {
        if (!TryObtenerUsuarioId(out var usuarioId))
        {
            return Unauthorized();
        }

        var propias = await notificaciones.ListarPropiasAsync(usuarioId, ct);
        return Ok(propias.Select(ARespuesta).ToList());
    }

    /// <summary>Marca como leída una notificación propia (RF-NOT-05).</summary>
    [HttpPost("{id:int}/leida")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarcarLeidaAsync(int id, CancellationToken ct)
    {
        if (!TryObtenerUsuarioId(out var usuarioId))
        {
            return Unauthorized();
        }

        try
        {
            var marcada = await notificaciones.MarcarLeidaAsync(usuarioId, id, ct);
            return Ok(ARespuesta(marcada));
        }
        catch (ReglaNegocioExcepcion ex)
        {
            return RespuestaDeError(ex);
        }
    }

    /// <summary>Devuelve si el correo de las notificaciones está activado (RF-NOT-07).</summary>
    [HttpGet("preferencias")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObtenerPreferenciaAsync(CancellationToken ct)
    {
        if (!TryObtenerUsuarioId(out var usuarioId))
        {
            return Unauthorized();
        }

        var correoActivo = await notificaciones.ObtenerPreferenciaAsync(usuarioId, ct);
        return Ok(new { correoActivo });
    }

    /// <summary>Actualiza la preferencia de correo (RF-NOT-07, RD-07).</summary>
    [HttpPut("preferencias")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CambiarPreferenciaAsync(
        ActualizarPreferenciaRequest peticion,
        CancellationToken ct)
    {
        if (!TryObtenerUsuarioId(out var usuarioId))
        {
            return Unauthorized();
        }

        // bool? distingue "no enviado" de false; sin esto, una omisión parecería un desactivar.
        if (peticion.CorreoActivo is null)
        {
            return BadRequest(new { mensaje = "correoActivo es obligatorio." });
        }

        await notificaciones.CambiarPreferenciaAsync(usuarioId, peticion.CorreoActivo.Value, ct);
        return Ok(new { correoActivo = peticion.CorreoActivo.Value });
    }

    /// <summary>RD-11: las fechas se serializan en UTC con sufijo Z.</summary>
    private static NotificacionRespuesta ARespuesta(Core.Notificaciones.Domain.Entidades.Notificacion notificacion)
        => new(
            notificacion.Id,
            notificacion.Tipo,
            notificacion.Titulo,
            notificacion.Mensaje,
            notificacion.Leida,
            DateTime.SpecifyKind(notificacion.FechaCreacion, DateTimeKind.Utc),
            notificacion.FechaLectura is null
                ? null
                : DateTime.SpecifyKind(notificacion.FechaLectura.Value, DateTimeKind.Utc));

    private bool TryObtenerUsuarioId(out int usuarioId)
        => int.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out usuarioId);

    private IActionResult RespuestaDeError(ReglaNegocioExcepcion ex)
        => StatusCode(ex.CodigoHttp, new { mensaje = ex.Message });

    public sealed record ActualizarPreferenciaRequest(bool? CorreoActivo);

    public sealed record NotificacionRespuesta(
        int Id,
        Core.Notificaciones.Domain.Enums.TipoNotificacion Tipo,
        string Titulo,
        string Mensaje,
        bool Leida,
        DateTime FechaCreacion,
        DateTime? FechaLectura);
}
