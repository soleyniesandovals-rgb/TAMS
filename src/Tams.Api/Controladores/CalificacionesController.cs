using System.IdentityModel.Tokens.Jwt;
using Core.ControlAcceso.Domain.Enums;
using Core.ControlAcceso.Domain.Excepciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tams.Api.Seguridad;
using Tams.Negocio.Application.Modelos;
using Tams.Negocio.Application.Servicios;
using Tams.Negocio.Domain.Enums;
using TransicionNoPermitidaCoreExcepcion = Core.Permisos.Domain.Excepciones.TransicionNoPermitidaExcepcion;
using TransicionNoPermitidaNegocioExcepcion = Tams.Negocio.Domain.Excepciones.TransicionNoPermitidaExcepcion;

namespace Tams.Api.Controladores;

/// <summary>
/// Operaciones del módulo de calificaciones (RF-NEG-02 a RF-NEG-06).
/// RD-02: la lógica de negocio vive en <see cref="ICalificacionesServicio"/>, no aquí.
/// RD-06: todas las operaciones exigen el rol Estándar del lado del servidor.
/// RD-07: el cuerpo de cada operación se valida antes de llamar al servicio.
/// </summary>
[ApiController]
[Authorize]
[RequiereRol(RolUsuario.Estandar)]
[Route("api/calificaciones")]
public class CalificacionesController(ICalificacionesServicio calificaciones) : ControllerBase
{
    /// <summary>Registra una calificación en estado Borrador (RF-NEG-02).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CalificacionResumen), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegistrarAsync(
        RegistrarCalificacionRequest peticion,
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

        return await EjecutarAsync(
            () => calificaciones.RegistrarAsync(
                usuarioId,
                peticion.EstudianteId,
                peticion.MateriaId,
                peticion.AnioEscolarId,
                peticion.TipoEvaluacion,
                peticion.Periodo,
                peticion.NumeroRa,
                peticion.PuntajeObtenido,
                ct),
            StatusCodes.Status201Created);
    }

    /// <summary>Envía una calificación en borrador (Borrador → Enviada, RF-NEG-03).</summary>
    [HttpPost("{id:int}/enviar")]
    [ProducesResponseType(typeof(CalificacionResumen), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> EnviarAsync(int id, CancellationToken ct)
    {
        if (!TryObtenerUsuarioId(out var usuarioId))
        {
            return Unauthorized();
        }

        return await EjecutarAsync(
            () => calificaciones.EnviarAsync(usuarioId, id, ct),
            StatusCodes.Status200OK);
    }

    /// <summary>Publica una calificación enviada (Enviada → Publicada, RF-NEG-03).</summary>
    [HttpPost("{id:int}/publicar")]
    [ProducesResponseType(typeof(CalificacionResumen), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> PublicarAsync(int id, CancellationToken ct)
    {
        if (!TryObtenerUsuarioId(out var usuarioId))
        {
            return Unauthorized();
        }

        return await EjecutarAsync(
            () => calificaciones.PublicarAsync(usuarioId, id, ct),
            StatusCodes.Status200OK);
    }

    /// <summary>
    /// Envía a recuperación una calificación publicada con puntaje bajo el mínimo
    /// (Publicada → EnRecuperacion, RF-NEG-03).
    /// </summary>
    [HttpPost("{id:int}/recuperacion")]
    [ProducesResponseType(typeof(CalificacionResumen), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> EnviarARecuperacionAsync(int id, CancellationToken ct)
    {
        if (!TryObtenerUsuarioId(out var usuarioId))
        {
            return Unauthorized();
        }

        return await EjecutarAsync(
            () => calificaciones.EnviarARecuperacionAsync(usuarioId, id, ct),
            StatusCodes.Status200OK);
    }

    /// <summary>
    /// Cierra una calificación publicada o en recuperación (RF-NEG-03, RF-NEG-05).
    /// </summary>
    [HttpPost("{id:int}/cerrar")]
    [ProducesResponseType(typeof(CalificacionResumen), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CerrarAsync(
        int id,
        CerrarCalificacionRequest peticion,
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

        return await EjecutarAsync(
            () => calificaciones.CerrarAsync(
                usuarioId,
                id,
                peticion.PuntajeRecuperacion,
                ct),
            StatusCodes.Status200OK);
    }

    /// <summary>
    /// Corrige el puntaje de una calificación publicada sin cambiar su estado
    /// (RF-NEG-02, RF-NEG-06). El servicio exige además el permiso de corrección.
    /// </summary>
    [HttpPatch("{id:int}/puntaje")]
    [ProducesResponseType(typeof(CalificacionResumen), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CorregirPuntajeAsync(
        int id,
        CorregirPuntajeRequest peticion,
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

        return await EjecutarAsync(
            () => calificaciones.CorregirPuntajeAsync(
                usuarioId,
                id,
                peticion.Puntaje,
                ct),
            StatusCodes.Status200OK);
    }

    /// <summary>Consulta las calificaciones del docente autenticado (RF-NEG-02).</summary>
    [HttpGet("mias")]
    [ProducesResponseType(typeof(IReadOnlyList<CalificacionResumen>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ConsultarMisCalificacionesAsync(CancellationToken ct)
    {
        if (!TryObtenerUsuarioId(out var usuarioId))
        {
            return Unauthorized();
        }

        return await EjecutarAsync(
            () => calificaciones.ConsultarMisCalificacionesAsync(usuarioId, ct),
            StatusCodes.Status200OK);
    }

    /// <summary>
    /// RD-08: ejecuta la operación del servicio y traduce sus excepciones de negocio a
    /// respuestas HTTP controladas, sin trazas ni nombres de clases.
    /// </summary>
    private async Task<IActionResult> EjecutarAsync<T>(
        Func<Task<T>> operacion,
        int codigoExito)
    {
        try
        {
            return StatusCode(codigoExito, await operacion());
        }
        catch (Exception ex) when (EsExcepcionDeNegocio(ex))
        {
            return RespuestaDeError(ex);
        }
    }

    private static bool EsExcepcionDeNegocio(Exception ex)
        => ex is ReglaNegocioExcepcion
            or TransicionNoPermitidaCoreExcepcion
            or TransicionNoPermitidaNegocioExcepcion;

    /// <summary>
    /// Punto único de traducción: <see cref="ReglaNegocioExcepcion"/> conserva su código
    /// HTTP y ambas transiciones no permitidas responden 409 con el mensaje controlado.
    /// </summary>
    private IActionResult RespuestaDeError(Exception ex)
    {
        if (ex is ReglaNegocioExcepcion regla)
        {
            return StatusCode(regla.CodigoHttp, new { mensaje = regla.Message });
        }

        if (ex is TransicionNoPermitidaCoreExcepcion or TransicionNoPermitidaNegocioExcepcion)
        {
            return Conflict(new { mensaje = ex.Message });
        }

        throw ex;
    }

    private bool TryObtenerUsuarioId(out int usuarioId)
        => int.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out usuarioId);

    public sealed record RegistrarCalificacionRequest(
        int EstudianteId,
        int MateriaId,
        int AnioEscolarId,
        TipoEvaluacion TipoEvaluacion,
        int? Periodo,
        int? NumeroRa,
        decimal PuntajeObtenido);

    public sealed record CerrarCalificacionRequest(decimal? PuntajeRecuperacion = null);

    public sealed record CorregirPuntajeRequest(decimal Puntaje);
}
