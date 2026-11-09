using Core.ControlAcceso.Domain.Excepciones;
using Microsoft.EntityFrameworkCore;
using Tams.Negocio.Application.Excepciones;
using Tams.Negocio.Application.Servicios;
using Tams.Negocio.Domain.Entidades;
using Tams.Negocio.Infrastructura.Persistence;
using Xunit;

namespace Tests.Negocio;

/// <summary>
/// RD-12: cálculo de días lectivos con un doble de <see cref="IProveedorFeriados"/>,
/// sin acceso a la red.
/// </summary>
public class CalendarioEscolarServicioTests
{
    [Fact]
    public async Task CalcularDiasLectivos_NoDescuentaFeriadoEnSabado()
    {
        // Preparar
        await using var contexto = CrearContexto();
        var anio = await SembrarAnioAsync(contexto, new DateOnly(2026, 8, 17), new DateOnly(2026, 8, 21));
        var proveedor = new ProveedorFeriadosFalso(_ =>
            [(new DateOnly(2026, 8, 22), "Feriado de sábado")]);
        var servicio = new CalendarioEscolarServicio(contexto, proveedor);

        // Actuar
        var resumen = await servicio.CalcularDiasLectivosAsync(anio.Id);

        // Verificar
        Assert.Equal(5, resumen.DiasHabiles);
        Assert.Empty(resumen.FeriadosDescontados);
        Assert.Equal(5, resumen.DiasLectivos);
    }

    [Fact]
    public async Task CalcularDiasLectivos_DosFeriadosElMismoDiaCuentanUnaVez()
    {
        // Preparar
        await using var contexto = CrearContexto();
        var anio = await SembrarAnioAsync(contexto, new DateOnly(2026, 8, 17), new DateOnly(2026, 8, 21));
        var proveedor = new ProveedorFeriadosFalso(_ =>
        [
            (new DateOnly(2026, 8, 19), "Feriado A"),
            (new DateOnly(2026, 8, 19), "Feriado B"),
        ]);
        var servicio = new CalendarioEscolarServicio(contexto, proveedor);

        // Actuar
        var resumen = await servicio.CalcularDiasLectivosAsync(anio.Id);

        // Verificar
        var feriado = Assert.Single(resumen.FeriadosDescontados);
        Assert.Equal(new DateOnly(2026, 8, 19), feriado.Fecha);
        Assert.Equal(4, resumen.DiasLectivos);
    }

    [Fact]
    public async Task CalcularDiasLectivos_RangoQueCruzaDosAniosPideAmbosAnios()
    {
        // Preparar
        await using var contexto = CrearContexto();
        var anio = await SembrarAnioAsync(contexto, new DateOnly(2026, 12, 28), new DateOnly(2027, 1, 5));
        var proveedor = new ProveedorFeriadosFalso(_ => []);
        var servicio = new CalendarioEscolarServicio(contexto, proveedor);

        // Actuar
        await servicio.CalcularDiasLectivosAsync(anio.Id);

        // Verificar
        Assert.Equal([2026, 2027], proveedor.AniosSolicitados);
    }

    [Fact]
    public async Task CalcularDiasLectivos_SinClavePropagaServicioNoConfigurado()
    {
        // Preparar
        await using var contexto = CrearContexto();
        var anio = await SembrarAnioAsync(contexto, new DateOnly(2026, 8, 17), new DateOnly(2026, 8, 21));
        var proveedor = new ProveedorFeriadosFalso(_ =>
            throw new ServicioFeriadosNoConfiguradoExcepcion());
        var servicio = new CalendarioEscolarServicio(contexto, proveedor);

        // Actuar y verificar
        await Assert.ThrowsAsync<ServicioFeriadosNoConfiguradoExcepcion>(
            () => servicio.CalcularDiasLectivosAsync(anio.Id));
    }

    [Fact]
    public async Task CalcularDiasLectivos_AnioInexistenteDevuelveNoEncontrado()
    {
        // Preparar
        await using var contexto = CrearContexto();
        var proveedor = new ProveedorFeriadosFalso(_ => []);
        var servicio = new CalendarioEscolarServicio(contexto, proveedor);

        // Actuar
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(
            () => servicio.CalcularDiasLectivosAsync(99999));

        // Verificar
        Assert.Equal(404, excepcion.CodigoHttp);
    }

    private static TamsDbContext CrearContexto()
    {
        var opciones = new DbContextOptionsBuilder<TamsDbContext>()
            .UseInMemoryDatabase($"CalendarioEscolar-{Guid.NewGuid():N}")
            .Options;

        return new TamsDbContext(opciones);
    }

    private static async Task<AnioEscolar> SembrarAnioAsync(
        TamsDbContext contexto,
        DateOnly fechaInicio,
        DateOnly fechaFin)
    {
        var anio = new AnioEscolar
        {
            Nombre = $"Año-{Guid.NewGuid():N}",
            FechaInicio = fechaInicio,
            FechaFin = fechaFin,
            EsActivo = true,
        };

        contexto.AniosEscolares.Add(anio);
        await contexto.SaveChangesAsync();

        return anio;
    }

    private sealed class ProveedorFeriadosFalso(
        Func<int, IReadOnlyList<(DateOnly Fecha, string Nombre)>> feriados) : IProveedorFeriados
    {
        public List<int> AniosSolicitados { get; } = [];

        public Task<IReadOnlyList<(DateOnly Fecha, string Nombre)>> ObtenerFeriadosNacionalesAsync(
            int anio,
            CancellationToken cancellationToken = default)
        {
            AniosSolicitados.Add(anio);
            return Task.FromResult(feriados(anio));
        }
    }
}
