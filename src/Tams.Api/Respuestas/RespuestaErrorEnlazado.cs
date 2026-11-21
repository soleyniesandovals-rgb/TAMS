using Microsoft.AspNetCore.Mvc;

namespace Tams.Api.Respuestas;

/// <summary>
/// RD-07/RD-08: con <c>[ApiController]</c>, ASP.NET Core responde a un error de enlazado
/// o de validación del modelo con un <c>ValidationProblemDetails</c> que expone la
/// excepción interna de System.Text.Json (tipos <c>Tams.*</c>, nombres de controlador,
/// rutas <c>$.campo</c> y <c>traceId</c>). Esta fábrica sustituye esa respuesta por el
/// formato del sistema: 400 con <c>{ "mensaje": "..." }</c>. El mensaje es fijo: no se
/// copia ningún texto del error recibido, para no filtrar detalles de la implementación.
/// </summary>
public static class RespuestaErrorEnlazado
{
    /// <summary>Mensaje controlado, común a cualquier error de entrada.</summary>
    public const string Mensaje = "La solicitud contiene datos inválidos.";

    /// <summary>
    /// Convierte un ModelState inválido en un 400 con un único campo <c>mensaje</c>.
    /// Método público y estático para poder probarse sin levantar la aplicación (RD-12).
    /// </summary>
    public static IActionResult Crear(ActionContext contexto)
    {
        // El contexto se recibe porque así lo exige InvalidModelStateResponseFactory;
        // deliberadamente NO se lee ningún error: su contenido puede incluir rutas,
        // tipos internos y trazas que RD-08 prohíbe devolver.
        _ = contexto;

        return new BadRequestObjectResult(new { mensaje = Mensaje });
    }
}
