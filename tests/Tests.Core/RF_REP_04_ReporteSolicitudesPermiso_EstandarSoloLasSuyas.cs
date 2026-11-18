using Core.ControlAcceso.Domain.Enums;
using Core.Permisos.Domain.Enums;
using Tests.Core.Dobles;

namespace Tests.Core;

/// <summary>
/// C-22 (RF-REP-04): un Estándar solo cuenta sus propias solicitudes; el
/// Administrador cuenta todas.
/// </summary>
public class RF_REP_04_ReporteSolicitudesPermiso_EstandarSoloLasSuyas
{
    [Fact]
    public async Task RF_REP_04_Estandar_SoloVeSusSolicitudesYAdministradorVeTodas()
    {
        // Preparar: dos Estándar con solicitudes en el mismo mes y estado.
        await using var escenario = ContextoReportes.Crear();
        await ContextoReportes.SembrarAsync(escenario.Contexto, escenario.Reloj, 1, EstadoSolicitudPermiso.Pendiente,
            new DateTimeOffset(2026, 9, 4, 10, 0, 0, TimeSpan.Zero));
        await ContextoReportes.SembrarAsync(escenario.Contexto, escenario.Reloj, 1, EstadoSolicitudPermiso.Pendiente,
            new DateTimeOffset(2026, 9, 5, 10, 0, 0, TimeSpan.Zero));
        await ContextoReportes.SembrarAsync(escenario.Contexto, escenario.Reloj, 2, EstadoSolicitudPermiso.Pendiente,
            new DateTimeOffset(2026, 9, 6, 10, 0, 0, TimeSpan.Zero));
        await ContextoReportes.SembrarAsync(escenario.Contexto, escenario.Reloj, 2, EstadoSolicitudPermiso.Pendiente,
            new DateTimeOffset(2026, 9, 7, 10, 0, 0, TimeSpan.Zero));
        await ContextoReportes.SembrarAsync(escenario.Contexto, escenario.Reloj, 2, EstadoSolicitudPermiso.Pendiente,
            new DateTimeOffset(2026, 9, 8, 10, 0, 0, TimeSpan.Zero));

        // Actuar: consulta el Estándar 1 y, después, un Administrador.
        var propias = await escenario.Servicio.ObtenerPorMesYEstadoAsync(
            1, RolUsuario.Estandar, "2026-09-01", "2026-09-30", CancellationToken.None);
        var todas = await escenario.Servicio.ObtenerPorMesYEstadoAsync(
            1, RolUsuario.Administrador, "2026-09-01", "2026-09-30", CancellationToken.None);

        // Verificar: el Estándar solo cuenta las suyas (2); el Administrador, las 5.
        Assert.Equal([("2026-09", "Pendiente", 2)], propias.Select(f => (f.Mes, f.Estado, f.Cantidad)).ToList());
        Assert.Equal([("2026-09", "Pendiente", 5)], todas.Select(f => (f.Mes, f.Estado, f.Cantidad)).ToList());
    }
}
