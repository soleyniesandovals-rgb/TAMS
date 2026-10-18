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
    public async Task CrearAsync_NaceEnEstadoPendiente()
    {
        var sut = CrearServicio();
        const int solicitanteId = 1;
        const string tipoPermiso = "acceso-reportes";

        var resultado = await sut.Servicio.CrearAsync(solicitanteId, tipoPermiso, CancellationToken.None);

        Assert.Equal(solicitanteId, resultado.SolicitanteId);
        Assert.Equal(tipoPermiso, resultado.TipoPermiso);
        Assert.Equal(EstadoSolicitudPermiso.Pendiente.ToString(), resultado.Estado);
        Assert.Null(resultado.ResueltaPorId);
        Assert.Null(resultado.FechaResolucion);
        Assert.Null(resultado.MotivoRechazo);
    }

    [Fact]
    public async Task AprobarAsync_TransicionaAAplicadaYRegistraQuienYCuando()
    {
        var sut = CrearServicio();
        const int solicitanteId = 2;
        const int administradorId = 3;
        const string tipoPermiso = "acceso-reportes";

        await sut.Servicio.CrearAsync(solicitanteId, tipoPermiso, CancellationToken.None);
        var solicitudes = sut.Solicitudes.ObtenerTodas();
        var solicitudId = solicitudes[0].Id;

        var resultado = await sut.Servicio.AprobarAsync(solicitudId, administradorId, CancellationToken.None);

        Assert.Equal(EstadoSolicitudPermiso.Aplicada.ToString(), resultado.Estado);
        Assert.Equal(administradorId, resultado.ResueltaPorId);
        Assert.NotNull(resultado.FechaResolucion);
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
    public async Task RechazarAsync_SinMotivoLanzaReglaNegocioExcepcion()
    {
        var sut = CrearServicio();
        const int solicitanteId = 2;
        const int administradorId = 3;
        const string tipoPermiso = "acceso-reportes";

        await sut.Servicio.CrearAsync(solicitanteId, tipoPermiso, CancellationToken.None);
        var solicitudes = sut.Solicitudes.ObtenerTodas();
        var solicitudId = solicitudes[0].Id;

        await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() =>
            sut.Servicio.RechazarAsync(solicitudId, administradorId, string.Empty, CancellationToken.None));
    }

    [Fact]
    public async Task RechazarAsync_ConMotivoTransicionaARechazada()
    {
        var sut = CrearServicio();
        const int solicitanteId = 2;
        const int administradorId = 3;
        const string tipoPermiso = "acceso-reportes";
        const string motivo = "No corresponde al cargo.";

        await sut.Servicio.CrearAsync(solicitanteId, tipoPermiso, CancellationToken.None);
        var solicitudes = sut.Solicitudes.ObtenerTodas();
        var solicitudId = solicitudes[0].Id;

        var resultado = await sut.Servicio.RechazarAsync(solicitudId, administradorId, motivo, CancellationToken.None);

        Assert.Equal(EstadoSolicitudPermiso.Rechazada.ToString(), resultado.Estado);
        Assert.Equal(motivo, resultado.MotivoRechazo);
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

    [Fact]
    public async Task ListarPropiasAsync_SoloDevuelveLasDelUsuario()
    {
        var sut = CrearServicio();
        const int usuario1 = 1;
        const int usuario2 = 2;

        await sut.Servicio.CrearAsync(usuario1, "permiso1", CancellationToken.None);
        await sut.Servicio.CrearAsync(usuario1, "permiso2", CancellationToken.None);
        await sut.Servicio.CrearAsync(usuario2, "permiso3", CancellationToken.None);

        var propiasUsuario1 = await sut.Servicio.ListarPropiasAsync(usuario1, CancellationToken.None);
        var propiasUsuario2 = await sut.Servicio.ListarPropiasAsync(usuario2, CancellationToken.None);

        Assert.Equal(2, propiasUsuario1.Count);
        Assert.All(propiasUsuario1, p => Assert.Equal(usuario1, p.SolicitanteId));

        Assert.Single(propiasUsuario2);
        Assert.All(propiasUsuario2, p => Assert.Equal(usuario2, p.SolicitanteId));
    }
}
