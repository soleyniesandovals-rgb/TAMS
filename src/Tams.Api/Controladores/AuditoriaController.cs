using Core.Auditoria.Application.Interfaces;
using Core.ControlAcceso.Domain.Enums;
using Core.ControlAcceso.Domain.Excepciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tams.Api.Seguridad;

namespace Tams.Api.Controladores;

/// <summary>
/// Consulta del historial de auditoría solo para el Administrador
/// (RF-AUD-06, RF-AUD-07, RD-06, RD-08). RD-02: el controlador solo recibe los
/// filtros como texto, llama al servicio y traduce el resultado a HTTP.
/// CA-14: expone únicamente GET; no hay PUT, PATCH ni DELETE.
/// </summary>
[ApiController]
[Route("api/auditoria")]
[Authorize]
public class AuditoriaController(IConsultaAuditoriaServicio consulta) : ControllerBase
{
    /// <summary>
    /// Devuelve el historial con los filtros opcionales y combinables (CA-15 a CA-18).
    /// Un error de regla se responde como <c>{ "mensaje": "..." }</c> con 400 (RD-07, RD-08).
    /// </summary>
    [HttpGet]
    [RequiereRol(RolUsuario.Administrador)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListarAsync(
        [FromQuery] string? usuarioId,
        [FromQuery] string? entidad,
        [FromQuery] string? desde,
        [FromQuery] string? hasta,
        CancellationToken ct)
    {
        try
        {
            return Ok(await consulta.ListarAsync(usuarioId, entidad, desde, hasta, ct));
        }
        catch (ReglaNegocioExcepcion ex)
        {
            // RD-08: solo el mensaje controlado; sin trazas, rutas ni nombres de tipos.
            return StatusCode(ex.CodigoHttp, new { mensaje = ex.Message });
        }
    }
}
