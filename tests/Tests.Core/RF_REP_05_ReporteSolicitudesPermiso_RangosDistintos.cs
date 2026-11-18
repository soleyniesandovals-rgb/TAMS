using Core.ControlAcceso.Domain.Enums;
using Core.Permisos.Domain.Enums;
using Tests.Core.Dobles;

namespace Tests.Core;

/// <summary>
/// C-23 (RF-REP-05): con los mismos datos, dos rangos distintos producen
/// resultados distintos en el reporte de solicitudes.
/// </summary>
public class RF_REP_05_ReporteSolicitudesPermiso_RangosDistintos
{
    [Fact]
    public async Task RF_REP_05_DosRangosSobreLosMismosDatos_DevuelvenAgrupacionesDistintas()
    {
        // Preparar: una solicitud en septiembre y dos en noviembre.
        await using var escenario = ContextoReportes.Crear();
        await ContextoReportes.SembrarAsync(escenario.Contexto, escenario.Reloj, 1, EstadoSolicitudPermiso.Pendiente,
            new DateTimeOffset(2026, 9, 15, 10, 0, 0, TimeSpan.Zero));
        await ContextoReportes.SembrarAsync(escenario.Contexto, escenario.Reloj, 1, EstadoSolicitudPermiso.Aprobada,
            new DateTimeOffset(2026, 11, 15, 10, 0, 0, TimeSpan.Zero));
        await ContextoReportes.SembrarAsync(escenario.Contexto, escenario.Reloj, 1, EstadoSolicitudPermiso.Aprobada,
            new DateTimeOffset(2026, 11, 20, 10, 0, 0, TimeSpan.Zero));

        // Actuar: se consultan dos rangos diferentes sobre el mismo conjunto de datos.
        var septiembre = await escenario.Servicio.ObtenerPorMesYEstadoAsync(
            1, RolUsuario.Administrador, "2026-09-01", "2026-09-30", CancellationToken.None);
        var noviembre = await escenario.Servicio.ObtenerPorMesYEstadoAsync(
            1, RolUsuario.Administrador, "2026-11-01", "2026-11-30", CancellationToken.None);

        // Verificar: los resultados difieren en mes, estado y cantidad.
        Assert.Equal([("2026-09", "Pendiente", 1)], septiembre.Select(f => (f.Mes, f.Estado, f.Cantidad)).ToList());
        Assert.Equal([("2026-11", "Aprobada", 2)], noviembre.Select(f => (f.Mes, f.Estado, f.Cantidad)).ToList());
        Assert.NotEqual(septiembre, noviembre);
    }
}
