using Core.ControlAcceso.Domain.Enums;
using Tams.Negocio.Domain.Enums;
using Tests.Negocio.Dobles;
using Xunit;

namespace Tests.Negocio;

/// <summary>
/// C-23 (RF-REP-05): con los mismos datos, dos rangos distintos producen
/// resultados distintos en el reporte de asistencia.
/// </summary>
public class RF_REP_05_ReporteAsistencia_RangosDistintos
{
    [Fact]
    public async Task RF_REP_05_DosRangosSobreLosMismosDatos_DevuelvenTotalesDistintos()
    {
        // Preparar: una asistencia en octubre y dos en noviembre para el mismo estudiante.
        await using var escenario = ContextoReporteAsistencia.Crear();
        await ContextoReporteAsistencia.SembrarEscenarioAsync(escenario.Contexto);
        await ContextoReporteAsistencia.SembrarAsistenciaAsync(escenario.Contexto, 1, 1, new DateOnly(2026, 10, 5), EstadoAsistencia.Presente);
        await ContextoReporteAsistencia.SembrarAsistenciaAsync(escenario.Contexto, 1, 1, new DateOnly(2026, 11, 5), EstadoAsistencia.Presente);
        await ContextoReporteAsistencia.SembrarAsistenciaAsync(escenario.Contexto, 1, 1, new DateOnly(2026, 11, 6), EstadoAsistencia.Ausente);

        // Actuar: se consultan dos rangos sobre el mismo conjunto de datos.
        var octubre = await escenario.Servicio.ObtenerPorEstudianteYMateriaAsync(
            10, RolUsuario.Administrador, "2026-10-01", "2026-10-31", CancellationToken.None);
        var noviembre = await escenario.Servicio.ObtenerPorEstudianteYMateriaAsync(
            10, RolUsuario.Administrador, "2026-11-01", "2026-11-30", CancellationToken.None);

        // Verificar: totales y porcentajes diferentes.
        Assert.Equal(1, Assert.Single(octubre).Total);
        Assert.Equal(100.00m, Assert.Single(octubre).PorcentajeAsistencia);
        Assert.Equal(2, Assert.Single(noviembre).Total);
        Assert.Equal(50.00m, Assert.Single(noviembre).PorcentajeAsistencia);
        Assert.NotEqual(octubre, noviembre);
    }
}
