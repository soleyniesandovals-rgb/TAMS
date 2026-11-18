using Core.ControlAcceso.Application.Interfaces;
using Core.ControlAcceso.Domain.Enums;
using Core.ControlAcceso.Domain.Excepciones;
using Microsoft.AspNetCore.Mvc;
using Tams.Api.Seguridad;

namespace Tams.Api.Controladores;

/// <summary>
/// Consulta administrativa de la cola de correos (RF-NOT-11, RD-06, RD-08).
/// RD-02: el controlador solo recibe, llama al servicio y traduce el resultado a HTTP.
/// </summary>
[ApiController]
[Route("api/correos")]
public class CorreosController(IConsultaCorreosEnColaServicio correos) : ControllerBase
{
    /// <summary>
    /// Lista los correos encolados, opcionalmente filtrados por estado; solo Administrador.
    /// </summary>
    [RequiereRol(RolUsuario.Administrador)]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListarAsync([FromQuery] string? estado, CancellationToken ct)
    {
        try
        {
            var resultado = await correos.ListarAsync(estado, ct);
            return Ok(resultado);
        }
        catch (ReglaNegocioExcepcion ex)
        {
            return StatusCode(ex.CodigoHttp, new { mensaje = ex.Message });
        }
    }
}
