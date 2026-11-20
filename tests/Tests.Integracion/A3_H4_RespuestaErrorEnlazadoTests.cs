using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Tams.Api.Respuestas;
using Xunit;

namespace Tests.Integracion;

/// <summary>
/// A3-H4 (RD-07, RD-08): un error de enlazado o de validación del modelo se traduce
/// siempre a 400 con {"mensaje":"..."}, sin los detalles internos del
/// ValidationProblemDetails de ASP.NET Core. No levanta la aplicación (RD-12).
/// </summary>
public class A3_H4_RespuestaErrorEnlazadoTests
{
    [Fact]
    public void A3_H4_ConversionDeCorreoActivoInvalida_Devuelve400ConMensajeSinDetallesInternos()
    {
        // Preparar: el ModelState que produce System.Text.Json cuando "correoActivo"
        // llega como texto en lugar de booleano.
        var contexto = CrearContexto(
            "correoActivo",
            "The JSON value could not be converted to System.Boolean. "
            + "Path $.correoActivo | LineNumber: 0 | BytePositionInLine: 19. "
            + "TraceId: 0HN8TESTTRACEID");

        // Actuar: se convierte ese error con la fábrica registrada en la API.
        var resultado = RespuestaErrorEnlazado.Crear(contexto);

        // Verificar: 400 con un único campo "mensaje" y ningún detalle interno.
        var respuesta = Assert.IsType<BadRequestObjectResult>(resultado);
        Assert.Equal(400, respuesta.StatusCode);
        var json = JsonSerializer.Serialize(respuesta.Value);
        using var cuerpo = JsonDocument.Parse(json);
        var propiedad = Assert.Single(cuerpo.RootElement.EnumerateObject());
        Assert.Equal("mensaje", propiedad.Name);
        Assert.Equal(RespuestaErrorEnlazado.Mensaje, propiedad.Value.GetString());
        Assert.DoesNotContain("Tams.", json, StringComparison.Ordinal);
        Assert.DoesNotContain("Controladores", json, StringComparison.Ordinal);
        Assert.DoesNotContain("$.", json, StringComparison.Ordinal);
        Assert.DoesNotContain("traceId", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("The JSON value", json, StringComparison.Ordinal);
    }

    [Fact]
    public void A3_H4_OtroErrorDeModelo_TampocoExponeTipoControladorNiTrazas()
    {
        // Preparar: un error de validación con un mensaje que menciona la implementación.
        var contexto = CrearContexto(
            "tipoPermiso",
            "Tams.Api.Controladores.PermisosController: System.ArgumentException en $.tipoPermiso (traceId 0HN9)");

        // Actuar: se convierte con la misma fábrica, no con una excepción por campo.
        var resultado = RespuestaErrorEnlazado.Crear(contexto);

        // Verificar: el mismo cuerpo controlado, sin copiar nada del error original.
        var respuesta = Assert.IsType<BadRequestObjectResult>(resultado);
        Assert.Equal(400, respuesta.StatusCode);
        var json = JsonSerializer.Serialize(respuesta.Value);
        Assert.DoesNotContain("Tams.", json, StringComparison.Ordinal);
        Assert.DoesNotContain("Controladores", json, StringComparison.Ordinal);
        Assert.DoesNotContain("$.", json, StringComparison.Ordinal);
        Assert.DoesNotContain("traceId", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("0HN9", json, StringComparison.Ordinal);
    }

    private static ActionContext CrearContexto(string campo, string mensaje)
    {
        var modelState = new ModelStateDictionary();
        modelState.AddModelError(campo, mensaje);

        return new ActionContext(
            new DefaultHttpContext(),
            new RouteData(),
            new ActionDescriptor(),
            modelState);
    }
}
