using System.IdentityModel.Tokens.Jwt;
using Core.ControlAcceso.Domain.Enums;
using Core.ControlAcceso.Domain.Excepciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Tams.Api.Seguridad;
using Tams.Negocio.Application.Excepciones;
using Tams.Negocio.Application.Modelos;
using Tams.Negocio.Application.Servicios;

namespace Tams.Api.Controladores;

/// <summary>
/// Consulta de años escolares, cálculo de días lectivos, alta y activación
/// (RF-NEG-02, RD-03, RD-06, RD-07, RD-08, RD-12).
/// El controlador solo delega en <see cref="ICalendarioEscolarServicio"/> y
/// <see cref="IAniosEscolaresServicio"/>; las reglas viven en la capa de negocio.
/// </summary>
[ApiController]
[Authorize]
[RequiereRol(RolUsuario.Administrador)]
[Route("api/anios-escolares")]
public class AniosEscolaresController(
    ICalendarioEscolarServicio calendario,
    IAniosEscolaresServicio aniosEscolares) : ControllerBase
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

    /// <summary>Crea un año escolar inactivo (RF-NEG-02, RD-02).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(AnioEscolarResumen), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CrearAsync(
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] CrearAnioEscolarRequest? peticion,
        CancellationToken ct)
    {
        if (!TryObtenerUsuarioId(out _))
        {
            return Unauthorized();
        }

        if (peticion is null)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Datos inválidos",
                detail: "El cuerpo de la petición es obligatorio.");
        }

        try
        {
            var creado = await aniosEscolares.CrearAsync(
                peticion.Nombre ?? string.Empty,
                peticion.FechaInicio,
                peticion.FechaFin,
                ct);

            return StatusCode(StatusCodes.Status201Created, creado);
        }
        catch (ReglaNegocioExcepcion ex)
        {
            return Problem(
                statusCode: ex.CodigoHttp,
                title: TituloDeAlta(ex.CodigoHttp),
                detail: ex.Message);
        }
    }

    /// <summary>Activa un año escolar y desactiva el anterior (RF-NEG-02, RD-06).</summary>
    [HttpPost("{id:int}/activar")]
    [ProducesResponseType(typeof(AnioEscolarResumen), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivarAsync(int id, CancellationToken ct)
    {
        if (!TryObtenerUsuarioId(out _))
        {
            return Unauthorized();
        }

        try
        {
            return Ok(await aniosEscolares.ActivarAsync(id, ct));
        }
        catch (ReglaNegocioExcepcion ex)
        {
            return Problem(
                statusCode: ex.CodigoHttp,
                title: TituloDeAlta(ex.CodigoHttp),
                detail: ex.Message);
        }
    }

    private static string TituloDeAlta(int codigoHttp) => codigoHttp switch
    {
        StatusCodes.Status400BadRequest => "Datos inválidos",
        StatusCodes.Status404NotFound => "Año escolar no encontrado",
        StatusCodes.Status409Conflict => "Conflicto",
        _ => "No se pudo procesar la solicitud",
    };

    private bool TryObtenerUsuarioId(out int usuarioId)
        => int.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out usuarioId);
}

/// <summary>Datos del alta de un año escolar (fechas en formato aaaa-mm-dd).</summary>
/// <param name="Nombre">Nombre visible, por ejemplo "2027-2028".</param>
/// <param name="FechaInicio">Primer día del año escolar.</param>
/// <param name="FechaFin">Último día del año escolar.</param>
public sealed record CrearAnioEscolarRequest(
    string? Nombre,
    DateOnly FechaInicio,
    DateOnly FechaFin);
