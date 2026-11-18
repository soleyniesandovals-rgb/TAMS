using Core.ControlAcceso.Domain.Enums;
using Core.Permisos.Domain.Enums;
using Tests.Core.Dobles;

namespace Tests.Core;

/// <summary>
/// C-24 (RF-REP-05): el rango incluye ambos extremos; la solicitud del último
/// día a las 23:59 cuenta y la del día siguiente no.
/// </summary>
public class RF_REP_05_ReporteSolicitudesPermiso_UltimoDiaIncluido
{
    [Fact]
    public async Task RF_REP_05_SolicitudDelUltimoDiaASegundasAntesDeMedianoche_CuentaYLaDelDiaSiguienteNo()
    {
        // Preparar: solicitudes en el primer instante del primer día, a las 23:59
        // del último día y en la medianoche del día siguiente.
        await using var escenario = ContextoReportes.Crear();
        await ContextoReportes.SembrarAsync(escenario.Contexto, escenario.Reloj, 1, EstadoSolicitudPermiso.Pendiente,
            new DateTimeOffset(2026, 10, 1, 0, 0, 0, TimeSpan.Zero));
        await ContextoReportes.SembrarAsync(escenario.Contexto, escenario.Reloj, 1, EstadoSolicitudPermiso.Pendiente,
            new DateTimeOffset(2026, 10, 31, 23, 59, 0, TimeSpan.Zero));
        await ContextoReportes.SembrarAsync(escenario.Contexto, escenario.Reloj, 1, EstadoSolicitudPermiso.Pendiente,
            new DateTimeOffset(2026, 11, 1, 0, 0, 0, TimeSpan.Zero));

        // Actuar: el rango termina el 31 de octubre.
        var filas = await escenario.Servicio.ObtenerPorMesYEstadoAsync(
            1, RolUsuario.Administrador, "2026-10-01", "2026-10-31", CancellationToken.None);

        // Verificar: cuentan las dos primeras (incluidos ambos extremos), no la del 1 de noviembre.
        Assert.Equal([("2026-10", "Pendiente", 2)], filas.Select(f => (f.Mes, f.Estado, f.Cantidad)).ToList());
    }
}
