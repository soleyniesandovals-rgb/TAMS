using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Core.ControlAcceso.Domain.Enums;
using Core.ControlAcceso.Domain.Excepciones;
using Core.Documentos.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Tams.Api.Controladores;

/// <summary>
/// Carga, listado, descarga y borrado lógico de documentos (RF-DOC-01 a RF-DOC-04).
/// RD-02: las reglas de negocio viven en <see cref="IGestionDocumentosServicio"/>.
/// RD-06: el dueño y el rol se verifican en el servicio; aquí solo se leen los claims.
/// RD-08: los errores se traducen a mensajes controlados, sin rutas ni trazas.
/// </summary>
[ApiController]
[Route("api/documentos")]
public class DocumentosController(IGestionDocumentosServicio documentos) : ControllerBase
{
    /// <summary>Sube un archivo y devuelve sus datos públicos (RF-DOC-01).</summary>
    [Authorize]
    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SubirAsync([FromForm] IFormFile? archivo, CancellationToken ct)
    {
        if (!TryObtenerUsuarioId(out var usuarioId))
        {
            return Unauthorized();
        }

        if (archivo is null)
        {
            return BadRequest(new { mensaje = "El archivo es obligatorio." });
        }

        try
        {
            await using var contenido = archivo.OpenReadStream();

            var creado = await documentos.SubirAsync(
                usuarioId,
                archivo.FileName,
                archivo.ContentType,
                contenido,
                archivo.Length,
                ct);

            return StatusCode(StatusCodes.Status201Created, creado);
        }
        catch (ReglaNegocioExcepcion ex)
        {
            return RespuestaDeError(ex);
        }
    }

    /// <summary>
    /// Lista los documentos visibles para el usuario autenticado (RF-DOC-02):
    /// los propios, o todos si es Administrador.
    /// </summary>
    [Authorize]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ListarAsync(CancellationToken ct)
    {
        if (!TryObtenerUsuarioId(out var usuarioId) || !TryObtenerRol(out var rol))
        {
            return Unauthorized();
        }

        return Ok(await documentos.ListarAsync(usuarioId, rol, ct));
    }

    /// <summary>
    /// Descarga el archivo con su tipo de contenido y su nombre original (RF-DOC-03).
    /// </summary>
    [Authorize]
    [HttpGet("{id:int}/descargar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DescargarAsync(int id, CancellationToken ct)
    {
        if (!TryObtenerUsuarioId(out var usuarioId) || !TryObtenerRol(out var rol))
        {
            return Unauthorized();
        }

        try
        {
            var descarga = await documentos.DescargarAsync(usuarioId, rol, id, ct);
            return File(descarga.Contenido, descarga.TipoContenido, descarga.Nombre);
        }
        catch (ReglaNegocioExcepcion ex)
        {
            return RespuestaDeError(ex);
        }
    }

    /// <summary>Marca un documento como eliminado; el archivo físico se conserva (RF-DOC-04).</summary>
    [Authorize]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EliminarAsync(int id, CancellationToken ct)
    {
        if (!TryObtenerUsuarioId(out var usuarioId) || !TryObtenerRol(out var rol))
        {
            return Unauthorized();
        }

        try
        {
            await documentos.EliminarAsync(usuarioId, rol, id, ct);
            return NoContent();
        }
        catch (ReglaNegocioExcepcion ex)
        {
            return RespuestaDeError(ex);
        }
    }

    private bool TryObtenerUsuarioId(out int usuarioId)
        => int.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out usuarioId);

    private bool TryObtenerRol(out RolUsuario rol)
        => Enum.TryParse(User.FindFirst(ClaimTypes.Role)?.Value, out rol);

    private IActionResult RespuestaDeError(ReglaNegocioExcepcion ex)
        => StatusCode(ex.CodigoHttp, new { mensaje = ex.Message });
}
