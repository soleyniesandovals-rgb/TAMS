using Core.ControlAcceso.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Tams.Api.Controladores;
using Tams.Api.Seguridad;
using Xunit;

namespace Tests.Integracion;

/// <summary>
/// RF-NEG (RD-06): las rutas del docente aceptan a Estándar y a Administrador,
/// siempre autorizadas en el servidor. La titularidad se comprueba después en el
/// servicio con el usuarioId del token; el atributo no amplía el acceso a los datos.
/// </summary>
public class RF_NEG_ControladoresDocenteAdministradorTests
{
    public static TheoryData<Type> Controladores => new()
    {
        typeof(DocenteController),
        typeof(AsistenciasController),
        typeof(CalificacionesController),
    };

    [Theory]
    [MemberData(nameof(Controladores))]
    public void RF_NEG_ControladorDocente_AdmiteEstandarYAdministrador(Type tipoControlador)
    {
        // Preparar: el tipo del controlador cuya ruta se va a autorizar.

        // Actuar: se lee el atributo de rol declarado en la clase.
        var atributo = Assert.IsType<RequiereRolAttribute>(
            Assert.Single(tipoControlador.GetCustomAttributes(typeof(RequiereRolAttribute), inherit: true)));

        // Verificar: exactamente los dos roles permitidos, en la política del servidor.
        var rolesDeclarados = atributo.Roles ?? string.Empty;
        Assert.Equal(
            new[] { RolUsuario.Estandar.ToString(), RolUsuario.Administrador.ToString() },
            rolesDeclarados.Split(','));
        Assert.NotEmpty(tipoControlador.GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true));
    }
}
