using Tams.Negocio.Domain;
using Xunit;

namespace Tests.Negocio;

/// <summary>
/// Pruebas de la regla de acumulado por estudiante (RF-NEG, RD-02). No usan base de datos.
/// </summary>
public class ReglasAcumuladoTests
{
    [Fact]
    public void RF_NEG_AcumuladoPeriodos_DosNotas_DevuelveElPromedio()
    {
        // Preparar
        decimal[] efectivos = [80m, 90m];

        // Actuar
        var acumulado = ReglasAcumulado.AcumuladoPeriodos(efectivos);

        // Verificar
        Assert.Equal(85m, acumulado);
    }

    [Fact]
    public void RF_NEG_AcumuladoPeriodos_UnaSolaNota_DevuelveEsaNota()
    {
        // Preparar
        decimal[] efectivos = [75m];

        // Actuar
        var acumulado = ReglasAcumulado.AcumuladoPeriodos(efectivos);

        // Verificar
        Assert.Equal(75m, acumulado);
    }

    [Fact]
    public void RF_NEG_AcumuladoPeriodos_SinNotas_DevuelveNull()
    {
        // Preparar
        decimal[] efectivos = [];

        // Actuar
        var acumulado = ReglasAcumulado.AcumuladoPeriodos(efectivos);

        // Verificar
        Assert.Null(acumulado);
    }

    [Fact]
    public void RD_02_AcumuladoPeriodos_RedondeaDosDecimalesAwayFromZero()
    {
        // Preparar
        decimal[] exacto = [80m, 85m, 90m];
        decimal[] periodico = [70m, 71m, 71m];

        // Actuar
        var primerAcumulado = ReglasAcumulado.AcumuladoPeriodos(exacto);
        var segundoAcumulado = ReglasAcumulado.AcumuladoPeriodos(periodico);

        // Verificar
        Assert.Equal(85m, primerAcumulado);
        Assert.Equal(70.67m, segundoAcumulado);
    }

    [Fact]
    public void RF_NEG_PuntajeEfectivo_ConRecuperacion_UsaLaRecuperacion()
    {
        // Preparar
        const decimal obtenido = 60m;
        const decimal recuperacion = 75m;

        // Actuar
        var efectivo = ReglasAcumulado.PuntajeEfectivo(obtenido, recuperacion);

        // Verificar
        Assert.Equal(75m, efectivo);
    }

    [Fact]
    public void RF_NEG_PuntajeEfectivo_SinRecuperacion_UsaElPuntajeObtenido()
    {
        // Preparar
        const decimal obtenido = 60m;

        // Actuar
        var efectivo = ReglasAcumulado.PuntajeEfectivo(obtenido, null);

        // Verificar
        Assert.Equal(60m, efectivo);
    }

    [Fact]
    public void RF_NEG_AcumuladoRa_UnRa_PonderaPorSusPuntos()
    {
        // Preparar
        (decimal PuntajeEfectivo, int Puntos)[] notas = [(80m, 34)];

        // Actuar
        var acumulado = ReglasAcumulado.AcumuladoRa(notas);

        // Verificar
        Assert.Equal(27.2m, acumulado);
    }

    [Fact]
    public void RF_NEG_AcumuladoRa_DosRa_SumaLosPuntosAcumulados()
    {
        // Preparar
        (decimal PuntajeEfectivo, int Puntos)[] notas = [(80m, 34), (70m, 33)];

        // Actuar
        var acumulado = ReglasAcumulado.AcumuladoRa(notas);

        // Verificar
        Assert.Equal(50.30m, acumulado);
    }

    [Fact]
    public void RF_NEG_AcumuladoRa_TresRaAlMaximo_DevuelveCien()
    {
        // Preparar
        (decimal PuntajeEfectivo, int Puntos)[] notas = [(100m, 34), (100m, 33), (100m, 33)];

        // Actuar
        var acumulado = ReglasAcumulado.AcumuladoRa(notas);

        // Verificar
        Assert.Equal(100m, acumulado);
    }

    [Fact]
    public void RF_NEG_AcumuladoRa_SinNotas_DevuelveNull()
    {
        // Preparar
        (decimal PuntajeEfectivo, int Puntos)[] notas = [];

        // Actuar
        var acumulado = ReglasAcumulado.AcumuladoRa(notas);

        // Verificar
        Assert.Null(acumulado);
    }
}
