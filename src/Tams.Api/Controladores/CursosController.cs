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
/// Alta y listado de cursos y de los estudiantes de cada curso
/// (RF-NEG-01, RF-NEG-02, RD-02, RD-06, RD-07, RD-08, RD-12).
/// El controlador solo delega en <see cref="ICursosServicio"/>; las reglas viven
/// en la capa de negocio.
/// </summary>
[ApiController]
[Authorize]
[RequiereRol(RolUsuario.Administrador)]
[Route("api/cursos")]
public class CursosController(ICursosServicio cursos) : ControllerBase
{
    /// <summary>Crea un curso dentro de un año escolar.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CursoResumen), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CrearCursoAsync(
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] CrearCursoRequest? peticion,
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
            var creado = await cursos.CrearCursoAsync(
                peticion.Grado ?? string.Empty,
                peticion.Seccion ?? string.Empty,
                peticion.EsTecnico,
                peticion.TipoTecnico,
                peticion.AnioEscolarId,
                ct);

            return StatusCode(StatusCodes.Status201Created, creado);
        }
        catch (ReglaNegocioExcepcion ex)
        {
            return StatusCode(ex.CodigoHttp, new { mensaje = ex.Message });
        }
    }

    /// <summary>Lista los cursos, opcionalmente filtrados por año escolar.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CursoResumen>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListarCursosAsync(
        [FromQuery] int? anioEscolarId,
        CancellationToken ct)
    {
        if (!TryObtenerUsuarioId(out _))
        {
            return Unauthorized();
        }

        return Ok(await cursos.ListarCursosAsync(anioEscolarId, ct));
    }

    /// <summary>Crea un estudiante con su cédula en un curso existente.</summary>
    [HttpPost("{cursoId:int}/estudiantes")]
    [ProducesResponseType(typeof(EstudianteAdministracionResumen), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CrearEstudianteAsync(
        int cursoId,
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] CrearEstudianteRequest? peticion,
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
            var creado = await cursos.CrearEstudianteAsync(
                cursoId,
                peticion.Nombre ?? string.Empty,
                peticion.Apellido ?? string.Empty,
                peticion.Cedula,
                ct);

            return StatusCode(StatusCodes.Status201Created, creado);
        }
        catch (ReglaNegocioExcepcion ex)
        {
            return StatusCode(ex.CodigoHttp, new { mensaje = ex.Message });
        }
    }

    /// <summary>Lista los estudiantes de un curso, ordenados por apellido y nombre.</summary>
    [HttpGet("{cursoId:int}/estudiantes")]
    [ProducesResponseType(typeof(IReadOnlyList<EstudianteAdministracionResumen>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListarEstudiantesAsync(int cursoId, CancellationToken ct)
    {
        if (!TryObtenerUsuarioId(out _))
        {
            return Unauthorized();
        }

        try
        {
            return Ok(await cursos.ListarEstudiantesAsync(cursoId, ct));
        }
        catch (ReglaNegocioExcepcion ex)
        {
            return StatusCode(ex.CodigoHttp, new { mensaje = ex.Message });
        }
    }

    private bool TryObtenerUsuarioId(out int usuarioId)
        => int.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out usuarioId);
}

/// <summary>Datos del alta de un curso.</summary>
/// <param name="Grado">Grado del curso; no puede repetirse con la misma sección en el año.</param>
/// <param name="Seccion">Sección del curso dentro del grado.</param>
/// <param name="EsTecnico">Indica si el curso es técnico.</param>
/// <param name="TipoTecnico">Modalidad técnica; opcional aunque el curso sea técnico.</param>
/// <param name="AnioEscolarId">Año escolar al que pertenece el curso.</param>
public sealed record CrearCursoRequest(
    string? Grado,
    string? Seccion,
    bool EsTecnico,
    string? TipoTecnico,
    int AnioEscolarId);

/// <summary>Datos del alta de un estudiante.</summary>
/// <param name="Nombre">Nombre del estudiante.</param>
/// <param name="Apellido">Apellido del estudiante.</param>
/// <param name="Cedula">Cédula / NUI del acta de nacimiento, con o sin guiones.</param>
public sealed record CrearEstudianteRequest(
    string? Nombre,
    string? Apellido,
    string? Cedula);
