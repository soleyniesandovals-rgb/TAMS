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
/// Alta y listado de asignaciones de docentes a materias en cursos
/// (RF-NEG-02, RD-02, RD-06, RD-12). El controlador solo delega en
/// <see cref="IAsignacionesServicio"/>; las reglas viven en la capa de negocio.
/// </summary>
[ApiController]
[Authorize]
[RequiereRol(RolUsuario.Administrador)]
[Route("api/asignaciones")]
public class AsignacionesController(IAsignacionesServicio asignaciones) : ControllerBase
{
    /// <summary>Asigna un docente a una materia de un curso.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(AsignacionAdministracionResumen), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CrearAsync(
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] CrearAsignacionRequest? peticion,
        CancellationToken ct)
    {
        if (!TryObtenerUsuarioId(out _))
        {
            return Unauthorized();
        }

        if (peticion is null)
        {
            return BadRequest(new { mensaje = "El cuerpo de la petición es obligatorio." });
        }

        try
        {
            var creada = await asignaciones.CrearAsync(
                peticion.UsuarioId,
                peticion.MateriaId,
                peticion.CursoId,
                peticion.HorasSemanales,
                ct);

            return StatusCode(StatusCodes.Status201Created, creada);
        }
        catch (ReglaNegocioExcepcion ex)
        {
            return StatusCode(ex.CodigoHttp, new { mensaje = ex.Message });
        }
    }

    /// <summary>Lista las asignaciones, con filtros opcionales por curso y por docente.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AsignacionAdministracionResumen>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListarAsync(
        [FromQuery] int? cursoId,
        [FromQuery] int? usuarioId,
        CancellationToken ct)
    {
        if (!TryObtenerUsuarioId(out _))
        {
            return Unauthorized();
        }

        return Ok(await asignaciones.ListarAsync(cursoId, usuarioId, ct));
    }

    private bool TryObtenerUsuarioId(out int usuarioId)
        => int.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out usuarioId);
}

/// <summary>Datos del alta de una asignación docente.</summary>
/// <param name="UsuarioId">Id del usuario docente en el Core.</param>
/// <param name="MateriaId">Materia que se asigna.</param>
/// <param name="CursoId">Curso donde se imparte la materia.</param>
/// <param name="HorasSemanales">Horas semanales de la asignación, entre 1 y 40.</param>
public sealed record CrearAsignacionRequest(
    int UsuarioId,
    int MateriaId,
    int CursoId,
    int HorasSemanales);
