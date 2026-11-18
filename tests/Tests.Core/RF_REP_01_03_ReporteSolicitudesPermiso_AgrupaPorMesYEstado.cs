using Core.ControlAcceso.Domain.Enums;
using Core.Permisos.Domain.Enums;
using Tests.Core.Dobles;

namespace Tests.Core;

/// <summary>
/// C-20 (RF-REP-01, RF-REP-03): la respuesta son filas de mes/estado con su
/// cantidad, no el listado de solicitudes.
/// </summary>
public class RF_REP_01_03_ReporteSolicitudesPermiso_AgrupaPorMesYEstado
{
    [Fact]
    public async Task RF_REP_01_Administrador_SeisSolicitudesDosMesesDosEstados_DevuelveCuatroFilasConCantidades()
    {
        // Preparar: seis solicitudes repartidas en dos meses y dos estados.
        await using var escenario = ContextoReportes.Crear();
        await ContextoReportes.SembrarAsync(escenario.Contexto, escenario.Reloj, 1, EstadoSolicitudPermiso.Pendiente,
            new DateTimeOffset(2026, 9, 3, 10, 0, 0, TimeSpan.Zero));
        await ContextoReportes.SembrarAsync(escenario.Contexto, escenario.Reloj, 2, EstadoSolicitudPermiso.Pendiente,
            new DateTimeOffset(2026, 9, 11, 11, 30, 0, TimeSpan.Zero));
        await ContextoReportes.SembrarAsync(escenario.Contexto, escenario.Reloj, 1, EstadoSolicitudPermiso.Aprobada,
            new DateTimeOffset(2026, 9, 20, 8, 15, 0, TimeSpan.Zero));
        await ContextoReportes.SembrarAsync(escenario.Contexto, escenario.Reloj, 2, EstadoSolicitudPermiso.Pendiente,
            new DateTimeOffset(2026, 10, 2, 9, 0, 0, TimeSpan.Zero));
        await ContextoReportes.SembrarAsync(escenario.Contexto, escenario.Reloj, 1, EstadoSolicitudPermiso.Rechazada,
            new DateTimeOffset(2026, 10, 12, 16, 45, 0, TimeSpan.Zero));
        await ContextoReportes.SembrarAsync(escenario.Contexto, escenario.Reloj, 2, EstadoSolicitudPermiso.Rechazada,
            new DateTimeOffset(2026, 10, 28, 17, 0, 0, TimeSpan.Zero));

        // Actuar: el Administrador consulta el rango que cubre ambos meses.
        var filas = await escenario.Servicio.ObtenerPorMesYEstadoAsync(
            1, RolUsuario.Administrador, "2026-09-01", "2026-10-31", CancellationToken.None);

        // Verificar: a lo sumo cuatro filas, ordenadas por mes y estado, con sus cantidades.
        Assert.Equal(4, filas.Count);
        Assert.Equal(
            [
                ("2026-09", "Aprobada", 1),
                ("2026-09", "Pendiente", 2),
                ("2026-10", "Pendiente", 1),
                ("2026-10", "Rechazada", 2),
            ],
            filas.Select(f => (f.Mes, f.Estado, f.Cantidad)).ToList());
    }
}
