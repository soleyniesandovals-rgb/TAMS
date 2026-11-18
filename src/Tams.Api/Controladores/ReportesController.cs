using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Core.ControlAcceso.Domain.Enums;
using Core.ControlAcceso.Domain.Excepciones;
using Core.Reportes.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tams.Negocio.Application.Servicios;

namespace Tams.Api.Controladores;

/// <summary>
/// Consulta de los dos reportes con rango de fechas (RF-REP-01 a RF-REP-05,
/// RD-06, RD-07, RD-08). RD-02: el controlador solo lee claims, delega en los
/// servicios y traduce el resultado a HTTP.
/// RF-CA-05: se exige sesión válida con <c>[Authorize]</c>, sin <c>[RequiereRol]</c>:
/// cualquier rol autenticado puede consultar. El filtrado por rol lo hacen los
/// servicios (Administrador ve todo; Estándar ve solo sus solicitudes o sus
/// asignaciones), para que la regla no se repita ni dependa del controlador.
/// </summary>
[ApiController]
[Route("api/reportes")]
[Authorize]
public class ReportesController(
    IReporteSolicitudesPermisoServicio solicitudesPermiso,
    IReporteAsistenciaServicio asistencia) : ControllerBase
{
    /// <summary>
    /// Solicitudes de permiso agrupadas por mes (UTC) y estado
    /// (RF-REP-01, RF-REP-03, RF-REP-05).
    /// </summary>
    [HttpGet("solicitudes-permiso")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SolicitudesPermisoAsync(
        [FromQuery] string? desde,
        [FromQuery] string? hasta,
        CancellationToken ct)
    {
        if (!TryObtenerIdentidad(out var usuarioId, out var rol))
        {
            return Unauthorized();
        }

        try
        {
            // Un rango sin datos devuelve 200 con una lista vacía (C-25).
            return Ok(await solicitudesPermiso.ObtenerPorMesYEstadoAsync(
                usuarioId, rol, desde, hasta, ct));
        }
        catch (ReglaNegocioExcepcion ex)
        {
            // RD-08: solo el mensaje controlado; sin trazas, rutas ni consultas.
            return RespuestaDeError(ex);
        }
    }

    /// <summary>
    /// Porcentaje de asistencia por estudiante y materia
    /// (RF-REP-02, RF-REP-03, RF-REP-05).
    /// </summary>
    [HttpGet("asistencia")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AsistenciaAsync(
        [FromQuery] string? desde,
        [FromQuery] string? hasta,
        CancellationToken ct)
    {
        if (!TryObtenerIdentidad(out var usuarioId, out var rol))
        {
            return Unauthorized();
        }

        try
        {
            // Un rango sin datos devuelve 200 con una lista vacía (C-25).
            return Ok(await asistencia.ObtenerPorEstudianteYMateriaAsync(
                usuarioId, rol, desde, hasta, ct));
        }
        catch (ReglaNegocioExcepcion ex)
        {
            // RD-08: solo el mensaje controlado; sin trazas, rutas ni consultas.
            return RespuestaDeError(ex);
        }
    }

    private bool TryObtenerIdentidad(out int usuarioId, out RolUsuario rol)
    {
        usuarioId = 0;
        rol = default;

        return int.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out usuarioId)
            && Enum.TryParse(User.FindFirst(ClaimTypes.Role)?.Value, ignoreCase: true, out rol);
    }

    private IActionResult RespuestaDeError(ReglaNegocioExcepcion ex)
        => StatusCode(ex.CodigoHttp, new { mensaje = ex.Message });
}
