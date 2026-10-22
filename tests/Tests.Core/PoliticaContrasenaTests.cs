using Core.ControlAcceso.Application.Reglas;
using Core.ControlAcceso.Domain.Excepciones;
using Xunit;

namespace Tests.Core;

public class PoliticaContrasenaTests
{
    [Fact]
    public void Exigir_ContrasenaConLetrasYNumerosMayorA8_NoLanzaExcepcion()
    {
        // Arrange
        var contrasena = "Segura2026";

        // Act
        var excepcion = Record.Exception(() => PoliticaContrasena.Exigir(contrasena));

        // Assert
        Assert.Null(excepcion);
    }

    [Fact]
    public void Exigir_ContrasenaDeExactamente8Caracteres_NoLanzaExcepcion()
    {
        // Arrange
        var contrasena = "abcd1234"; // 8 caracteres exactos

        // Act
        var excepcion = Record.Exception(() => PoliticaContrasena.Exigir(contrasena));

        // Assert
        Assert.Null(excepcion);
    }

    [Fact]
    public void Exigir_ContrasenaDe5Caracteres_LanzaReglaNegocioExcepcion()
    {
        // Arrange
        var contrasena = "abc12"; // menos de 8

        // Act
        var excepcion = Record.Exception(() => PoliticaContrasena.Exigir(contrasena));

        // Assert
        Assert.IsType<ReglaNegocioExcepcion>(excepcion);
    }

    // Opcionales: cubren las otras ramas de la política

    [Fact]
    public void Exigir_ContrasenaLargaSinNumeros_LanzaReglaNegocioExcepcion()
    {
        // Arrange
        var contrasena = "soloLetrasAqui";

        // Act
        var excepcion = Record.Exception(() => PoliticaContrasena.Exigir(contrasena));

        // Assert
        Assert.IsType<ReglaNegocioExcepcion>(excepcion);
    }

    [Fact]
    public void Exigir_ContrasenaLargaSinLetras_LanzaReglaNegocioExcepcion()
    {
        // Arrange
        var contrasena = "123456789";

        // Act
        var excepcion = Record.Exception(() => PoliticaContrasena.Exigir(contrasena));

        // Assert
        Assert.IsType<ReglaNegocioExcepcion>(excepcion);
    }
}