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
/// Alta y listado de materias con sus resultados de aprendizaje
/// (RF-NEG-01, RF-NEG-02, RD-02, RD-06, RD-07, RD-08, RD-12).
/// El controlador solo delega en <see cref="IMateriasServicio"/>; las reglas viven
/// en la capa de negocio.
/// </summary>
[ApiController]
[Authorize]
[RequiereRol(RolUsuario.Administrador)]
[Route("api/materias")]
public class MateriasController(IMateriasServicio materias) : ControllerBase
{
    /// <summary>Crea una materia y, si es técnica, sus resultados de aprendizaje.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(MateriaResumen), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CrearAsync(
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] CrearMateriaRequest? peticion,
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
            var creada = await materias.CrearAsync(
                peticion.Nombre ?? string.Empty,
                peticion.EsTecnica,
                peticion.TipoTecnico,
                peticion.ResultadosAprendizaje?
                    .Select(r => new ResultadoAprendizajeEntrada(r.Numero, r.Descripcion, r.Puntos))
                    .ToList(),
                ct);

            return StatusCode(StatusCodes.Status201Created, creada);
        }
        catch (ReglaNegocioExcepcion ex)
        {
            return StatusCode(ex.CodigoHttp, new { mensaje = ex.Message });
        }
    }

    /// <summary>Lista todas las materias con sus resultados de aprendizaje.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<MateriaResumen>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListarAsync(CancellationToken ct)
    {
        if (!TryObtenerUsuarioId(out _))
        {
            return Unauthorized();
        }

        return Ok(await materias.ListarAsync(ct));
    }

    private bool TryObtenerUsuarioId(out int usuarioId)
        => int.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out usuarioId);
}

/// <summary>Datos del alta de una materia.</summary>
/// <param name="Nombre">Nombre visible; no puede repetirse.</param>
/// <param name="EsTecnica">Indica si la materia lleva resultados de aprendizaje.</param>
/// <param name="TipoTecnico">Modalidad técnica; obligatoria cuando <paramref name="EsTecnica"/> es true.</param>
/// <param name="ResultadosAprendizaje">Resultados de aprendizaje de una materia técnica.</param>
public sealed record CrearMateriaRequest(
    string? Nombre,
    bool EsTecnica,
    string? TipoTecnico,
    IReadOnlyList<CrearResultadoAprendizajeRequest>? ResultadosAprendizaje);

/// <summary>Datos de un resultado de aprendizaje dentro del alta de una materia.</summary>
public sealed record CrearResultadoAprendizajeRequest(
    int Numero,
    string? Descripcion,
    int Puntos);
