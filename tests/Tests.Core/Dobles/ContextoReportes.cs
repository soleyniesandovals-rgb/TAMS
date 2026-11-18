using Core.ControlAcceso.Infrastructura.Persistence;
using Core.ControlAcceso.Infrastructura.Repositorios;
using Core.Permisos.Domain.Entidades;
using Core.Permisos.Domain.Enums;
using Core.Reportes.Application.Servicios;
using Microsoft.EntityFrameworkCore;

namespace Tests.Core.Dobles;

/// <summary>
/// Contexto en memoria y reloj controlado para las pruebas del reporte del Core
/// (RD-12): base única por prueba y fechas de creación selladas por el reloj,
/// igual que hace <see cref="CoreDbContext"/> en producción (RD-11).
/// </summary>
internal static class ContextoReportes
{
    public static EscenarioReportes Crear()
    {
        var reloj = new RelojFijo(new DateTimeOffset(2026, 9, 1, 12, 0, 0, TimeSpan.Zero));
        var opciones = new DbContextOptionsBuilder<CoreDbContext>()
            .UseInMemoryDatabase($"reportes-solicitudes-{Guid.NewGuid():N}")
            .Options;

        var contexto = new CoreDbContext(opciones, reloj);
        var unidad = new CoreUnidadDeTrabajo(contexto);

        return new EscenarioReportes(contexto, reloj, new ReporteSolicitudesPermisoServicio(unidad));
    }

    /// <summary>Agrega una solicitud cuya fecha de creación es la del reloj.</summary>
    public static async Task<SolicitudPermiso> SembrarAsync(
        CoreDbContext contexto,
        RelojFijo reloj,
        int solicitanteId,
        EstadoSolicitudPermiso estado,
        DateTimeOffset fechaCreacion)
    {
        reloj.Instante = fechaCreacion;

        var solicitud = new SolicitudPermiso
        {
            SolicitanteId = solicitanteId,
            TipoPermiso = "acceso-reportes",
            Estado = estado,
        };

        contexto.SolicitudesPermiso.Add(solicitud);
        await contexto.SaveChangesAsync();

        return solicitud;
    }

    internal sealed class EscenarioReportes(
        CoreDbContext contexto,
        RelojFijo reloj,
        ReporteSolicitudesPermisoServicio servicio) : IAsyncDisposable
    {
        public CoreDbContext Contexto { get; } = contexto;

        public RelojFijo Reloj { get; } = reloj;

        public ReporteSolicitudesPermisoServicio Servicio { get; } = servicio;

        public ValueTask DisposeAsync() => Contexto.DisposeAsync();
    }

    internal sealed class RelojFijo(DateTimeOffset instante) : TimeProvider
    {
        public DateTimeOffset Instante { get; set; } = instante;

        public override DateTimeOffset GetUtcNow() => Instante;
    }
}
