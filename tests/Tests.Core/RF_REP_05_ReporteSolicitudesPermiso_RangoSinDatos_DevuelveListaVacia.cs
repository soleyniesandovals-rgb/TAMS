using Core.ControlAcceso.Domain.Enums;
using Core.Permisos.Domain.Enums;
using Tests.Core.Dobles;

namespace Tests.Core;

/// <summary>
/// C-25 (RF-REP-05, RD-07): un rango válido sin datos responde 200 con lista vacía.
/// </summary>
public class RF_REP_05_ReporteSolicitudesPermiso_RangoSinDatos_DevuelveListaVacia
{
    [Fact]
    public async Task RF_REP_05_RangoSinSolicitudes_DevuelveListaVacia()
    {
        // Preparar: una solicitud en octubre de 2026 y un rango en 2020.
        await using var escenario = ContextoReportes.Crear();
        await ContextoReportes.SembrarAsync(escenario.Contexto, escenario.Reloj, 1, EstadoSolicitudPermiso.Pendiente,
            new DateTimeOffset(2026, 10, 15, 10, 0, 0, TimeSpan.Zero));

        // Actuar: se consulta un rango donde no hay solicitudes.
        var filas = await escenario.Servicio.ObtenerPorMesYEstadoAsync(
            1, RolUsuario.Administrador, "2020-01-01", "2020-01-31", CancellationToken.None);

        // Verificar: lista vacía, sin errores.
        Assert.Empty(filas);
    }
}
