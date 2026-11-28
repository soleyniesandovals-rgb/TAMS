using Tams.Negocio.Domain;
using Xunit;

namespace Tests.Negocio;

/// <summary>
/// Pruebas de la regla pura de advertencia por mínimo de RA (RF-NEG, RD-02).
/// No usan base de datos.
/// </summary>
public class ReglasCalificacionTests
{
    [Fact]
    public void RF_NEG_AdvertenciaMinimoRa_SinMinimo_DevuelveNull()
    {
        // Preparar: el RA no define mínimo.
        const int numeroRa = 1;
        const decimal puntaje = 60m;
        int? puntajeMinimo = null;

        // Actuar: se evalúa la regla pura del dominio.
        var advertencia = ReglasCalificacion.AdvertenciaMinimoRa(numeroRa, puntaje, puntajeMinimo);

        // Verificar: sin mínimo no hay advertencia.
        Assert.Null(advertencia);
    }

    [Fact]
    public void RF_NEG_AdvertenciaMinimoRa_IgualAlMinimo_DevuelveNull()
    {
        // Preparar: la nota alcanza exactamente el mínimo.
        const int numeroRa = 1;
        const decimal puntaje = 70m;
        const int puntajeMinimo = 70;

        // Actuar: se evalúa la regla pura del dominio.
        var advertencia = ReglasCalificacion.AdvertenciaMinimoRa(numeroRa, puntaje, puntajeMinimo);

        // Verificar: igualar el mínimo no genera advertencia.
        Assert.Null(advertencia);
    }

    [Fact]
    public void RF_NEG_AdvertenciaMinimoRa_PorEncimaDelMinimo_DevuelveNull()
    {
        // Preparar: la nota supera el mínimo.
        const int numeroRa = 2;
        const decimal puntaje = 85m;
        const int puntajeMinimo = 70;

        // Actuar: se evalúa la regla pura del dominio.
        var advertencia = ReglasCalificacion.AdvertenciaMinimoRa(numeroRa, puntaje, puntajeMinimo);

        // Verificar: por encima del mínimo no hay advertencia.
        Assert.Null(advertencia);
    }

    [Fact]
    public void RF_NEG_AdvertenciaMinimoRa_PorDebajoDelMinimo_DevuelveAdvertencia()
    {
        // Preparar: la nota queda por debajo del mínimo del RA.
        const int numeroRa = 1;
        const decimal puntaje = 60m;
        const int puntajeMinimo = 70;

        // Actuar: se evalúa la regla pura del dominio.
        var advertencia = ReglasCalificacion.AdvertenciaMinimoRa(numeroRa, puntaje, puntajeMinimo);

        // Verificar: el texto nombra el RA y el mínimo.
        Assert.Equal("La nota del RA1 está por debajo del mínimo de 70 puntos.", advertencia);
    }
}
