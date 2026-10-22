using Core.ControlAcceso.Domain.Excepciones;
using Core.Permisos.Application.Servicios;
using Core.Permisos.Domain.Entidades;
using Core.Permisos.Domain.Enums;
using Core.Permisos.Domain.Excepciones;
using Tests.Permisos.Dobles;
using Xunit;

namespace Tests.Permisos;

public class GestionPermisosServicioTests
{
    private readonly TimeProviderFijo _timeProvider = TimeProviderFijo.CrearUtc(2026, 10, 17, 12, 0, 0);

    private (GestionPermisosServicio Servicio, CoreUnidadDeTrabajoFalsa Unidad, SolicitudesPermisoRepositorioFalso Solicitudes, PermisosActivosRepositorioFalso PermisosActivos) CrearServicio()
    {
        var solicitudes = new SolicitudesPermisoRepositorioFalso();
        var permisosActivos = new PermisosActivosRepositorioFalso();
        var unidad = new CoreUnidadDeTrabajoFalsa(solicitudes, permisosActivos);
        var servicio = new GestionPermisosServicio(unidad, _timeProvider);
        return (servicio, unidad, solicitudes, permisosActivos);
    }

    [Fact]
    public async Task AprobarAsync_CreaPermisoActivo()
    {
        var sut = CrearServicio();
        const int solicitanteId = 2;
        const int administradorId = 3;
        const string tipoPermiso = "acceso-reportes";

        await sut.Servicio.CrearAsync(solicitanteId, tipoPermiso, CancellationToken.None);
        var solicitudes = sut.Solicitudes.ObtenerTodas();
        var solicitudId = solicitudes[0].Id;

        await sut.Servicio.AprobarAsync(solicitudId, administradorId, CancellationToken.None);

        var permisosActivos = sut.PermisosActivos.ObtenerTodos();
        Assert.Single(permisosActivos);
        Assert.Equal(solicitanteId, permisosActivos[0].UsuarioId);
        Assert.Equal(tipoPermiso, permisosActivos[0].TipoPermiso);
        Assert.Equal(solicitudId, permisosActivos[0].SolicitudPermisoId);
    }

    [Fact]
    public async Task AprobarAsync_SegundaVezLanzaYEstadoNoCambia()
    {
        var sut = CrearServicio();
        const int solicitanteId = 2;
        const int administradorId = 3;
        const string tipoPermiso = "acceso-reportes";

        await sut.Servicio.CrearAsync(solicitanteId, tipoPermiso, CancellationToken.None);
        var solicitudes = sut.Solicitudes.ObtenerTodas();
        var solicitudId = solicitudes[0].Id;

        var resultadoPrimeraAprobacion = await sut.Servicio.AprobarAsync(solicitudId, administradorId, CancellationToken.None);
        Assert.Equal(EstadoSolicitudPermiso.Aplicada.ToString(), resultadoPrimeraAprobacion.Estado);

        await Assert.ThrowsAsync<TransicionNoPermitidaExcepcion>(() =>
            sut.Servicio.AprobarAsync(solicitudId, administradorId, CancellationToken.None));

        var solicitudFinal = await sut.Solicitudes.BuscarPorIdAsync(solicitudId);
        Assert.NotNull(solicitudFinal);
        Assert.Equal(EstadoSolicitudPermiso.Aplicada, solicitudFinal.Estado);
    }

    [Fact]
    public async Task AprobarAsync_PropiaSolicitudLanzaReglaNegocioExcepcion()
    {
        var sut = CrearServicio();
        const int administradorId = 3;
        const string tipoPermiso = "acceso-reportes";

        await sut.Servicio.CrearAsync(administradorId, tipoPermiso, CancellationToken.None);
        var solicitudes = sut.Solicitudes.ObtenerTodas();
        var solicitudId = solicitudes[0].Id;

        await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() =>
            sut.Servicio.AprobarAsync(solicitudId, administradorId, CancellationToken.None));
    }

    /// <summary>RF-GP-04/RF-GP-06: una solicitud ya Aplicada no puede rechazarse.</summary>
    [Fact]
    public async Task RechazarAsync_SobreSolicitudYaAplicadaLanzaTransicionNoPermitida()
    {
        var sut = CrearServicio();
        const int solicitanteId = 1;
        const int administradorId = 2;

        await sut.Servicio.CrearAsync(solicitanteId, "acceso-reportes", CancellationToken.None);
        var solicitudId = sut.Solicitudes.ObtenerTodas()[0].Id;
        await sut.Servicio.AprobarAsync(solicitudId, administradorId, CancellationToken.None);

        await Assert.ThrowsAsync<TransicionNoPermitidaExcepcion>(() =>
            sut.Servicio.RechazarAsync(solicitudId, administradorId, "Motivo.", CancellationToken.None));

        var solicitudFinal = await sut.Solicitudes.BuscarPorIdAsync(solicitudId);
        Assert.NotNull(solicitudFinal);
        Assert.Equal(EstadoSolicitudPermiso.Aplicada, solicitudFinal.Estado);
    }

    /// <summary>RF-GP-07: nadie resuelve su propia solicitud, tampoco al rechazarla.</summary>
    [Fact]
    public async Task RechazarAsync_PropiaSolicitudLanzaReglaNegocioExcepcion()
    {
        var sut = CrearServicio();
        const int administradorId = 3;

        await sut.Servicio.CrearAsync(administradorId, "acceso-reportes", CancellationToken.None);
        var solicitudId = sut.Solicitudes.ObtenerTodas()[0].Id;

        await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() =>
            sut.Servicio.RechazarAsync(solicitudId, administradorId, "Motivo.", CancellationToken.None));
    }
}
