using System.Reflection;
using System.Text.Json;
using Core.Auditoria.Application.Modelos;
using Core.Auditoria.Domain.Enums;
using Core.ControlAcceso.Domain.Enums;
using Core.ControlAcceso.Domain.Excepciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tams.Api.Controladores;
using Tams.Api.Seguridad;
using Tests.Integracion.Dobles;
using Xunit;

namespace Tests.Integracion;

/// <summary>
/// RF-AUD-06/RF-AUD-07 (CA-14, CA-17, RD-06, RD-08): la API expone solo el GET de
/// consulta, exige sesión y rol Administrador en el servidor, y devuelve errores como
/// { "mensaje": "..." }. Sin levantar la aplicación ni usar SQL Server (RD-12).
/// </summary>
public class AuditoriaControllerTests
{
    [Fact]
    public async Task RF_AUD_07_GetSinFiltros_PasaLosFiltrosAlServicioYDevuelve200()
    {
        // Preparar: un servicio doble con un registro ya validado.
        var esperado = new RegistroAuditoriaResumen(
            1, 3, AccionAuditoria.CambiarRol, EntidadAuditoria.Usuario, 9,
            new DateTime(2026, 11, 21, 23, 59, 0, DateTimeKind.Utc), "Estándar", "Administrador");
        var servicio = new ConsultaAuditoriaServicioFalsa { Resultado = [esperado] };
        var controlador = new AuditoriaController(servicio);

        // Actuar: se invoca el único GET con los cuatro filtros como texto.
        var resultado = await controlador.ListarAsync(
            "3", "Usuario", "2026-11-01", "2026-11-30", CancellationToken.None);

        // Verificar: CA-15 el controlador solo delega y traduce a 200 sin tocar los datos.
        var ok = Assert.IsType<OkObjectResult>(resultado);
        Assert.Same(esperado, Assert.Single(Assert.IsAssignableFrom<IReadOnlyList<RegistroAuditoriaResumen>>(ok.Value)));
        Assert.Equal("3", servicio.UsuarioIdRecibido);
        Assert.Equal("Usuario", servicio.EntidadRecibida);
        Assert.Equal("2026-11-01", servicio.DesdeRecibido);
        Assert.Equal("2026-11-30", servicio.HastaRecibido);
    }

    [Fact]
    public async Task RF_AUD_18_ErrorDeRegla_Devuelve400SoloConCampoMensaje()
    {
        // Preparar: un servicio que rechaza la entrada con un mensaje controlado (RD-07).
        var excepcion = new ReglaNegocioExcepcion("usuarioId debe ser un entero positivo.");
        var servicio = new ConsultaAuditoriaServicioFalsa { Excepcion = excepcion };
        var controlador = new AuditoriaController(servicio);

        // Actuar: se invoca el GET con un filtro inválido.
        var resultado = await controlador.ListarAsync("0", null, null, null, CancellationToken.None);

        // Verificar: CA-18/RD-08 400 con un único campo "mensaje", sin rutas ni traceId.
        var respuesta = Assert.IsType<ObjectResult>(resultado);
        Assert.Equal(400, respuesta.StatusCode);
        var json = JsonSerializer.Serialize(respuesta.Value);
        using var cuerpo = JsonDocument.Parse(json);
        var propiedad = Assert.Single(cuerpo.RootElement.EnumerateObject());
        Assert.Equal("mensaje", propiedad.Name);
        Assert.Equal(excepcion.Message, propiedad.Value.GetString());
        Assert.DoesNotContain("$.", json, StringComparison.Ordinal);
        Assert.DoesNotContain("traceId", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RF_AUD_06_Controlador_ExigeSesionYRolAdministradorEnElServidor()
    {
        // Preparar: el tipo del controlador y la exigencia declarada en el servidor.
        var tipo = typeof(AuditoriaController);

        // Actuar: se leen el atributo de autorización de clase y el de rol de la acción.
        var autorizacionesClase = tipo.GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true);
        var accion = Assert.Single(tipo.GetMethods(
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        var roles = Assert.IsType<RequiereRolAttribute>(
            Assert.Single(accion.GetCustomAttributes(typeof(RequiereRolAttribute), inherit: true)));

        // Verificar: CA-17/RD-06 sin token → 401 y Estándar → 403 por política del servidor.
        Assert.NotEmpty(autorizacionesClase);
        Assert.Equal(RolUsuario.Administrador.ToString(), roles.Roles);
        Assert.True(typeof(AuthorizeAttribute).IsAssignableFrom(typeof(RequiereRolAttribute)));
    }

    [Fact]
    public void RF_AUD_06_Controlador_SoloExponeGetEnLaRutaApiAuditoria()
    {
        // Preparar: el tipo del controlador y los verbos HTTP prohibidos (CA-14).
        var tipo = typeof(AuditoriaController);
        var verbosProhibidos = new Type[]
        {
            typeof(HttpPostAttribute),
            typeof(HttpPutAttribute),
            typeof(HttpPatchAttribute),
            typeof(HttpDeleteAttribute),
        };

        // Actuar: se listan sus métodos públicos y sus atributos de ruta y verbo.
        var metodos = tipo.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Select(m => m.Name)
            .ToList();
        var accion = Assert.Single(tipo.GetMethods(
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        var get = Assert.Single(accion.GetCustomAttributes(typeof(HttpGetAttribute), inherit: true));
        var prohibidosPresentes = verbosProhibidos
            .Where(atributo => accion.GetCustomAttributes(atributo, inherit: true).Any())
            .ToList();
        var ruta = Assert.Single(tipo.GetCustomAttributes<RouteAttribute>());

        // Verificar: CA-14 solo GET en api/auditoria; no hay PUT, PATCH ni DELETE.
        Assert.Equal(new[] { "ListarAsync" }, metodos);
        Assert.IsType<HttpGetAttribute>(get);
        Assert.Empty(prohibidosPresentes);
        Assert.Equal("api/auditoria", ruta.Template);
    }
}
