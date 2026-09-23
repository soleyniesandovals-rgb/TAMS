using NetArchTest.Rules;

namespace Tests.Arquitectura;

// RD-03: El Core no depende del módulo de negocio; la dependencia va en un solo sentido.
// Criterio de aceptación: si se elimina el módulo de negocio, el Core sigue construyéndose y ejecutándose.
public class ReglasArquitectura
{
    [Fact]
    public void Core_NoDebeDependerDe_TamsNegocio()
    {
        var resultado = Types
            .InAssembly(typeof(Core.AssemblyMarker).Assembly)
            .Should()
            .NotHaveDependencyOn("Tams.Negocio")
            .GetResult();

        Assert.True(
            resultado.IsSuccessful,
            "RD-03 violado. Tipos de Core que dependen de Tams.Negocio: "
            + string.Join(", ", resultado.FailingTypeNames ?? Enumerable.Empty<string>()));
    }
}
