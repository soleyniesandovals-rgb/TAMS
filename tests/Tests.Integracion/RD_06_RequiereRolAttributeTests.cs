using Core.ControlAcceso.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Tams.Api.Seguridad;
using Xunit;

namespace Tests.Integracion;

/// <summary>
/// RD-06: el atributo de autorización debe admitir uno o varios roles fuertemente
/// tipados y dejar en <see cref="AuthorizeAttribute.Roles"/> los nombres que espera
/// el motor de autorización de ASP.NET Core.
/// </summary>
public class RD_06_RequiereRolAttributeTests
{
    [Fact]
    public void RD_06_ConDosRoles_RolesTieneAmbosNombresSeparadosPorComa()
    {
        // Preparar: dos roles del enum, en el orden declarado en el controlador.
        var roles = new[] { RolUsuario.Estandar, RolUsuario.Administrador };

        // Actuar: se construye el atributo con el constructor de varios roles.
        var atributo = new RequiereRolAttribute(roles[0], roles[1]);

        // Verificar: la política del servidor acepta cualquiera de los dos roles.
        Assert.Equal("Estandar,Administrador", atributo.Roles);
        Assert.IsAssignableFrom<AuthorizeAttribute>(atributo);
    }

    [Fact]
    public void RD_06_ConUnRol_RolesMantieneElComportamientoAnterior()
    {
        // Preparar: un único rol; la sobrecarga de un rol debe seguir eligiéndose.
        var rol = RolUsuario.Administrador;

        // Actuar: se construye el atributo con la sobrecarga de un solo rol.
        var atributo = new RequiereRolAttribute(rol);

        // Verificar: el nombre del rol es el mismo que antes del cambio.
        Assert.Equal("Administrador", atributo.Roles);
        Assert.Equal(RolUsuario.Estandar.ToString(), new RequiereRolAttribute(RolUsuario.Estandar).Roles);
    }
}
