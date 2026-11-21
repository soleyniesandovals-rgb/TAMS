using Core.ControlAcceso.Application.Reglas;
using Core.ControlAcceso.Domain.Enums;

namespace Tests.Core;

/// <summary>
/// A3-H2 (C-17, RD-06, RD-07): el rol leído del token solo puede ser uno de los
/// NOMBRES de RolUsuario. Un número como "0" o "99" no debe tratarse como
/// Administrador; la regla vive en la pieza de Control de acceso.
/// </summary>
public class A3_H2_ReglasRolUsuarioTests
{
    [Theory]
    [InlineData("0")]
    [InlineData("1")]
    [InlineData("99")]
    [InlineData("Administrador,Estandar")]
    [InlineData("Perdido")]
    public void A3_H2_RolQueNoEsUnNombre_NoSeInterpretaComoAdministrador(string valor)
    {
        // Preparar: el texto que llega como claim de rol.
        // Actuar: se interpreta con la regla de Control de acceso.
        var rol = ReglasRolUsuario.Obtener(valor);

        // Verificar: no se reconoce ningún rol, y en particular no es Administrador.
        Assert.Null(rol);
        Assert.NotEqual(RolUsuario.Administrador, rol);
    }

    [Theory]
    [InlineData("Administrador", RolUsuario.Administrador)]
    [InlineData("administrador", RolUsuario.Administrador)]
    [InlineData("Estandar", RolUsuario.Estandar)]
    [InlineData("ESTANDAR", RolUsuario.Estandar)]
    [InlineData("  Estandar  ", RolUsuario.Estandar)]
    public void A3_H2_NombreDeRolValido_SeInterpretaSinDistinguirMayusculas(string valor, RolUsuario esperado)
    {
        // Preparar: un nombre válido del enum, con mayúsculas mixtas o espacios.
        // Actuar: se interpreta con la regla de Control de acceso.
        var rol = ReglasRolUsuario.Obtener(valor);

        // Verificar: se obtiene exactamente el rol esperado.
        Assert.Equal(esperado, rol);
    }
}
