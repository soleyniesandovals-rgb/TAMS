using System.IdentityModel.Tokens.Jwt;
using Core.ControlAcceso.Domain.Enums;
using Core.ControlAcceso.Domain.Excepciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tams.Api.Seguridad;
using Tams.Negocio.Application.Modelos;
using Tams.Negocio.Application.Servicios;

namespace Tams.Api.Controladores;

/// <summary>
/// Consultas de solo lectura para el docente (RF-NEG-02, RD-02, RD-06).
/// La lógica de negocio vive en <see cref="IConsultasDocenteServicio"/>, no aquí.
/// </summary>
[ApiController]
[Authorize]
[RequiereRol(RolUsuario.Estandar)]
[Route("api/docente")]
public class DocenteController(IConsultasDocenteServicio consultas) : ControllerBase
{
    /// <summary>Lista las asignaciones del docente autenticado.</summary>
    [HttpGet("asignaciones")]
    [ProducesResponseType(typeof(IReadOnlyList<AsignacionDocenteResumen>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListarMisAsignacionesAsync(CancellationToken ct)
    {
        if (!TryObtenerUsuarioId(out var usuarioId))
        {
            return Unauthorized();
        }

        return Ok(await consultas.ListarMisAsignacionesAsync(usuarioId, ct));
    }

    /// <summary>Lista los estudiantes de un curso asignado al docente autenticado.</summary>
    [HttpGet("cursos/{cursoId:int}/estudiantes")]
    [ProducesResponseType(typeof(IReadOnlyList<EstudianteResumen>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListarEstudiantesDeCursoAsync(int cursoId, CancellationToken ct)
    {
        if (!TryObtenerUsuarioId(out var usuarioId))
        {
            return Unauthorized();
        }

        try
        {
            return Ok(await consultas.ListarEstudiantesDeCursoAsync(usuarioId, cursoId, ct));
        }
        catch (ReglaNegocioExcepcion ex)
        {
            return StatusCode(ex.CodigoHttp, new { mensaje = ex.Message });
        }
    }

    /// <summary>Obtiene el año escolar activo.</summary>
    [HttpGet("anio-escolar-activo")]
    [ProducesResponseType(typeof(AnioEscolarResumen), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerAnioEscolarActivoAsync(CancellationToken ct)
    {
        if (!TryObtenerUsuarioId(out _))
        {
            return Unauthorized();
        }

        try
        {
            return Ok(await consultas.ObtenerAnioEscolarActivoAsync(ct));
        }
        catch (ReglaNegocioExcepcion ex)
        {
            return StatusCode(ex.CodigoHttp, new { mensaje = ex.Message });
        }
    }

    private bool TryObtenerUsuarioId(out int usuarioId)
        => int.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out usuarioId);
}
