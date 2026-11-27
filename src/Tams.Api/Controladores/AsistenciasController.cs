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
/// Consulta y guardado de la lista de asistencia del docente autenticado
/// (RF-NEG-02, RD-02, RD-06, RD-07, RD-12). El controlador solo delega en
/// <see cref="IAsistenciasServicio"/>; las reglas viven en la capa de negocio.
/// </summary>
[ApiController]
[Authorize]
[RequiereRol(RolUsuario.Estandar, RolUsuario.Administrador)]
[Route("api/docente/asistencias")]
public class AsistenciasController(IAsistenciasServicio asistencias) : ControllerBase
{
    /// <summary>Obtiene la lista de una asignación y fecha.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ListaAsistenciaResumen), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerListaAsync(
        [FromQuery] int? asignacionId,
        [FromQuery] DateOnly? fecha,
        CancellationToken ct)
    {
        if (!TryObtenerUsuarioId(out var usuarioId))
        {
            return Unauthorized();
        }

        if (!asignacionId.HasValue || !fecha.HasValue)
        {
            return BadRequest(new { mensaje = "asignacionId y fecha son obligatorios." });
        }

        try
        {
            return Ok(await asistencias.ObtenerListaAsync(
                usuarioId,
                asignacionId.Value,
                fecha.Value,
                ct));
        }
        catch (ReglaNegocioExcepcion ex)
        {
            return StatusCode(ex.CodigoHttp, new { mensaje = ex.Message });
        }
    }

    /// <summary>Guarda o corrige la lista completa de una asignación y fecha.</summary>
    [HttpPut]
    [ProducesResponseType(typeof(ListaAsistenciaResumen), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GuardarListaAsync(
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] GuardarListaAsistenciaRequest? peticion,
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
            return Ok(await asistencias.GuardarListaAsync(
                usuarioId,
                peticion.AsignacionId,
                peticion.Fecha,
                peticion.Registros,
                ct));
        }
        catch (ReglaNegocioExcepcion ex)
        {
            return StatusCode(ex.CodigoHttp, new { mensaje = ex.Message });
        }
    }

    private bool TryObtenerUsuarioId(out int usuarioId)
        => int.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out usuarioId);
}

/// <summary>Datos para guardar o corregir una lista de asistencia.</summary>
/// <param name="AsignacionId">Asignación docente (materia y curso) de la lista.</param>
/// <param name="Fecha">Fecha de la clase.</param>
/// <param name="Registros">Estado de cada estudiante del curso.</param>
public sealed record GuardarListaAsistenciaRequest(
    int AsignacionId,
    DateOnly Fecha,
    IReadOnlyList<RegistroAsistenciaEntrada>? Registros);
