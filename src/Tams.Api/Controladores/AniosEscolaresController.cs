using System.IdentityModel.Tokens.Jwt;
using Core.ControlAcceso.Domain.Enums;
using Core.ControlAcceso.Domain.Excepciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tams.Api.Seguridad;
using Tams.Negocio.Application.Excepciones;
using Tams.Negocio.Application.Modelos;
using Tams.Negocio.Application.Servicios;

namespace Tams.Api.Controladores;

/// <summary>
/// Consulta de años escolares y cálculo de días lectivos (RD-03, RD-06, RD-07, RD-08, RD-12).
/// El controlador solo delega en <see cref="ICalendarioEscolarServicio"/>; el cálculo y
/// el acceso al proveedor externo viven en las capas de negocio e infraestructura.
/// </summary>
[ApiController]
[Authorize]
[RequiereRol(RolUsuario.Administrador)]
[Route("api/anios-escolares")]
public class AniosEscolaresController(ICalendarioEscolarServicio calendario) : ControllerBase
{
    /// <summary>Lista los años escolares registrados.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AnioEscolarResumen>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListarAsync(CancellationToken ct)
    {
        if (!TryObtenerUsuarioId(out _))
        {
            return Unauthorized();
        }

        return Ok(await calendario.ListarAniosEscolaresAsync(ct));
    }

    /// <summary>
    /// Calcula los días lectivos del año escolar indicado. Un fallo del proveedor de
    /// feriados se traduce a un ProblemDetails controlado, sin trazas.
    /// </summary>
    [HttpGet("{id:int}/dias-lectivos")]
    [ProducesResponseType(typeof(DiasLectivosResumen), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> CalcularDiasLectivosAsync(int id, CancellationToken ct)
    {
        if (!TryObtenerUsuarioId(out _))
        {
            return Unauthorized();
        }

        try
        {
            return Ok(await calendario.CalcularDiasLectivosAsync(id, ct));
        }
        catch (ServicioFeriadosNoConfiguradoExcepcion ex)
        {
            return Problem(
                statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Servicio de feriados no configurado",
                detail: ex.Message);
        }
        catch (ServicioFeriadosNoDisponibleExcepcion ex)
        {
            return Problem(
                statusCode: StatusCodes.Status502BadGateway,
                title: "Servicio de feriados no disponible",
                detail: ex.Message);
        }
        catch (ReglaNegocioExcepcion ex)
        {
            return Problem(
                statusCode: ex.CodigoHttp,
                title: ex.CodigoHttp == StatusCodes.Status404NotFound
                    ? "Año escolar no encontrado"
                    : "No se pudo calcular los días lectivos",
                detail: ex.Message);
        }
    }

    private bool TryObtenerUsuarioId(out int usuarioId)
        => int.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out usuarioId);
}
