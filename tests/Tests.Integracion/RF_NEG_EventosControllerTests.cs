using System.Reflection;
using System.Security.Claims;
using Core.ControlAcceso.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Tams.Api.Controladores;
using Tams.Api.Seguridad;
using Xunit;
using Xunit.Sdk;

namespace Tests.Integracion;

/// <summary>
/// RF-NEG (RD-06): las rutas de eventos permiten leer a Estándar y Administrador,
/// pero crear y borrar exigen además Administrador. Se comprueba por reflexión y con
/// el motor de autorización real, sin levantar la aplicación ni usar red.
/// </summary>
public class RF_NEG_EventosControllerTests
{
    [Fact]
    public void RF_NEG_EventosController_AdmiteEstandarYAdministrador()
    {
        // Preparar: el tipo del controlador cuya ruta se va a autorizar.

        // Actuar: se lee el atributo de rol declarado en la clase.
        var atributo = Assert.IsType<RequiereRolAttribute>(
            Assert.Single(typeof(EventosController).GetCustomAttributes(typeof(RequiereRolAttribute), true)));

        // Verificar: exactamente los dos roles permitidos y autenticación obligatoria.
        Assert.Equal(
            [RolUsuario.Estandar.ToString(), RolUsuario.Administrador.ToString()],
            (atributo.Roles ?? string.Empty).Split(','));
        Assert.NotEmpty(typeof(EventosController).GetCustomAttributes(typeof(AuthorizeAttribute), true));
    }

    [Theory]
    [InlineData("CrearAsync")]
    [InlineData("BorrarAsync")]
    public void RF_NEG_EventosController_CrearYBorrar_ExigenAdministrador(string nombreMetodo)
    {
        // Preparar: el método que crea o borra un evento.

        // Actuar: se lee su atributo de rol.
        var atributo = Assert.IsType<RequiereRolAttribute>(
            Assert.Single(Metodo(nombreMetodo).GetCustomAttributes(typeof(RequiereRolAttribute), true)));

        // Verificar: solo Administrador puede escribir.
        Assert.Equal(
            [RolUsuario.Administrador.ToString()],
            (atributo.Roles ?? string.Empty).Split(','));
    }

    [Theory]
    [InlineData("ListarAsync")]
    [InlineData("ProximosAsync")]
    public void RF_NEG_EventosController_ListarYProximos_NoAnadenOtroRol(string nombreMetodo)
    {
        // Preparar: los métodos de solo lectura.

        // Actuar: se buscan atributos de rol en el método.

        // Verificar: hereda el de la clase y no añade otro.
        Assert.Empty(Metodo(nombreMetodo).GetCustomAttributes(typeof(RequiereRolAttribute), true));
    }

    [Theory]
    [InlineData("CrearAsync", "Estandar", false)]
    [InlineData("CrearAsync", "Administrador", true)]
    [InlineData("BorrarAsync", "Estandar", false)]
    [InlineData("BorrarAsync", "Administrador", true)]
    public async Task RF_NEG_EventosController_ClaseYMetodo_CombinanRolesYRechazanEstandar(
        string nombreMetodo,
        string rol,
        bool esperadoAutorizado)
    {
        // Preparar: los atributos de clase y método tal como los Combina ASP.NET Core,
        // un principal autenticado con el rol indicado y el motor de autorización real.
        var atributos = typeof(EventosController).GetCustomAttributes(inherit: true)
            .OfType<IAuthorizeData>()
            .Concat(Metodo(nombreMetodo).GetCustomAttributes(inherit: true).OfType<IAuthorizeData>())
            .ToList();
        var proveedorPoliticas = new DefaultAuthorizationPolicyProvider(
            Options.Create(new AuthorizationOptions()));
        var politica = await AuthorizationPolicy.CombineAsync(proveedorPoliticas, atributos);
        var servicios = new ServiceCollection();
        servicios.AddLogging();
        servicios.AddAuthorization();
        await using var proveedorServicios = servicios.BuildServiceProvider();
        var autorizador = proveedorServicios.GetRequiredService<IAuthorizationService>();
        var usuario = new ClaimsPrincipal(
            new ClaimsIdentity([new Claim(ClaimTypes.Role, rol)], "Prueba"));

        // Actuar: se evalúa la política combinada contra el usuario.
        var resultado = await autorizador.AuthorizeAsync(usuario, null, politica!);

        // Verificar: el Estándar es rechazado y el Administrador autorizado.
        Assert.Equal(esperadoAutorizado, resultado.Succeeded);
    }

    private static MethodInfo Metodo(string nombre)
        => typeof(EventosController).GetMethod(nombre)
           ?? throw new XunitException($"No existe el método {nombre} en EventosController.");
}
