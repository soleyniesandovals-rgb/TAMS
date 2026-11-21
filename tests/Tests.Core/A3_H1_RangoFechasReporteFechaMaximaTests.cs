using Core.ControlAcceso.Domain.Enums;
using Core.Permisos.Domain.Enums;
using Core.Reportes.Application;
using Tests.Core.Dobles;

namespace Tests.Core;

/// <summary>
/// A3-H1 (C-24, C-25, RD-07, RD-08): una fecha con formato válido nunca debe
/// provocar una excepción al calcular el rango del reporte, incluido el último
/// día representable. Sin levantar la aplicación ni usar SQL Server (RD-12).
/// </summary>
public class A3_H1_RangoFechasReporteFechaMaximaTests
{
    [Fact]
    public void A3_H1_HastaUltimoDiaRepresentable_CreaRangoSinExcepcionYSinLimiteSuperior()
    {
        // Preparar: los extremos dados por el usuario, ambos en 9999-12-31.
        const string fecha = "9999-12-31";

        // Actuar: se construye el rango con esa fecha válida.
        var rango = RangoFechasReporte.Crear(fecha, fecha);

        // Verificar: no hay excepción, el último día sigue siendo el extremo inclusive
        // y no existe un límite superior representable.
        Assert.Equal(new DateOnly(9999, 12, 31), rango.Desde);
        Assert.Equal(new DateOnly(9999, 12, 31), rango.Hasta);
        Assert.Equal(DateTime.MaxValue, rango.HastaExclusivoUtc);
    }

    [Fact]
    public async Task A3_H1_ServicioConHastaUltimoDiaYRangoSinDatos_DevuelveListaVacia()
    {
        // Preparar: un reporte sobre una base vacía.
        await using var escenario = ContextoReportes.Crear();

        // Actuar: se consulta con desde y hasta iguales al último día representable.
        var filas = await escenario.Servicio.ObtenerPorMesYEstadoAsync(
            1, RolUsuario.Administrador, "9999-12-31", "9999-12-31", CancellationToken.None);

        // Verificar: la respuesta es la de un rango sin datos (200 con lista vacía), sin excepciones.
        Assert.Empty(filas);
    }

    [Fact]
    public async Task A3_H1_ServicioConHastaUltimoDia_SolicitudDelMismoDiaSigueIncluida()
    {
        // Preparar: una solicitud creada el último día del rango, casi a medianoche.
        await using var escenario = ContextoReportes.Crear();
        await ContextoReportes.SembrarAsync(escenario.Contexto, escenario.Reloj, 1,
            EstadoSolicitudPermiso.Pendiente,
            new DateTimeOffset(9999, 12, 31, 23, 59, 0, TimeSpan.Zero));

        // Actuar: el rango termina en ese mismo último día representable.
        var filas = await escenario.Servicio.ObtenerPorMesYEstadoAsync(
            1, RolUsuario.Administrador, "9999-12-31", "9999-12-31", CancellationToken.None);

        // Verificar: el extremo superior sigue incluido aunque no haya límite superior.
        Assert.Equal([("9999-12", "Pendiente", 1)], filas.Select(f => (f.Mes, f.Estado, f.Cantidad)).ToList());
    }

    [Fact]
    public async Task A3_H1_RangoNormal_UltimoDiaIncluidoYDiaSiguienteExcluido()
    {
        // Preparar: una solicitud el último día del rango y otra justo al día siguiente.
        await using var escenario = ContextoReportes.Crear();
        await ContextoReportes.SembrarAsync(escenario.Contexto, escenario.Reloj, 1,
            EstadoSolicitudPermiso.Pendiente,
            new DateTimeOffset(2026, 10, 31, 23, 59, 0, TimeSpan.Zero));
        await ContextoReportes.SembrarAsync(escenario.Contexto, escenario.Reloj, 1,
            EstadoSolicitudPermiso.Pendiente,
            new DateTimeOffset(2026, 11, 1, 0, 0, 0, TimeSpan.Zero));

        // Actuar: se consulta un rango normal que termina el 31 de octubre.
        var filas = await escenario.Servicio.ObtenerPorMesYEstadoAsync(
            1, RolUsuario.Administrador, "2026-10-01", "2026-10-31", CancellationToken.None);

        // Verificar: C-24 se mantiene; solo cuenta la del último día incluido.
        Assert.Equal([("2026-10", "Pendiente", 1)], filas.Select(f => (f.Mes, f.Estado, f.Cantidad)).ToList());
    }
}
