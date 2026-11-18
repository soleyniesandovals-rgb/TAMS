using Core.ControlAcceso.Domain.Enums;
using Core.ControlAcceso.Domain.Excepciones;
using Core.Reportes.Application;
using Tests.Core.Dobles;

namespace Tests.Core;

/// <summary>
/// C-25 (RF-REP-05, RD-07): desde y hasta son obligatorios, van en formato
/// exacto AAAA-MM-DD y desde no puede ser posterior a hasta.
/// </summary>
public class RF_REP_05_RangoFechasReporte_ValidaExtremos
{
    [Theory]
    [InlineData(null, "2026-10-31", "desde")]
    [InlineData("2026-09-01", null, "hasta")]
    [InlineData("", "2026-10-31", "desde")]
    [InlineData("2026-9-01", "2026-10-31", "AAAA-MM-DD")]
    [InlineData("2026-09-01 ", "2026-10-31", "AAAA-MM-DD")]
    [InlineData("2026-09-01", "31/10/2026", "AAAA-MM-DD")]
    [InlineData("2026-11-01", "2026-10-31", "posterior")]
    public void RF_REP_05_EntradaInvalida_LanzaErrorControlado400(string? desde, string? hasta, string textoEsperado)
    {
        // Actuar: se intenta construir un rango con una entrada inválida.
        var excepcion = Assert.Throws<ReglaNegocioExcepcion>(() => RangoFechasReporte.Crear(desde, hasta));

        // Verificar: es un 400 con mensaje claro, sin trazas ni detalles técnicos.
        Assert.Equal(400, excepcion.CodigoHttp);
        Assert.Contains(textoEsperado, excepcion.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void RF_REP_05_RangoValido_ConvierteExtremosEnUtcYExcluyeSoloElDiaSiguiente()
    {
        // Actuar: se convierte un rango de dos meses con formato exacto.
        var rango = RangoFechasReporte.Crear("2026-09-01", "2026-10-31");

        // Verificar: ambos extremos inclusive y límite superior exclusivo en UTC.
        Assert.Equal(new DateOnly(2026, 9, 1), rango.Desde);
        Assert.Equal(new DateOnly(2026, 10, 31), rango.Hasta);
        Assert.Equal(new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc), rango.DesdeUtc);
        Assert.Equal(new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc), rango.HastaExclusivoUtc);
    }

    [Fact]
    public async Task RF_REP_05_ServicioConRangoInvalido_Propaga400SinConsultar()
    {
        // Preparar: un servicio sobre una base vacía.
        await using var escenario = ContextoReportes.Crear();

        // Actuar: se consulta con un extremo ausente.
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() =>
            escenario.Servicio.ObtenerPorMesYEstadoAsync(
                1, RolUsuario.Administrador, null, "2026-10-31", CancellationToken.None));

        // Verificar: la entrada inválida da 400 controlado con mensaje claro.
        Assert.Equal(400, excepcion.CodigoHttp);
        Assert.Equal("desde es obligatorio con formato AAAA-MM-DD.", excepcion.Message);
    }
}
