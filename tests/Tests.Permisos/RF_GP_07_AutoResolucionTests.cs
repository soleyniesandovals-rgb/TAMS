using Core.ControlAcceso.Domain.Excepciones;
using Core.Permisos.Domain.Enums;
using Tests.Permisos.Dobles;
using Xunit;

namespace Tests.Permisos;

/// <summary>
/// RF-GP-07: prohibicion de auto-resolucion.
/// Ni al aprobar ni al rechazar, un Administrador puede resolver una solicitud propia.
/// </summary>
public class AutoResolucionTests
{
    [Fact]
    public async Task RF_GP_07_Aprobar_PropiaSolicitud_LanzaReglaNegocio409YEstadoNoCambia()
    {
        // Preparar: el Administrador crea su propia solicitud.
        var contexto = ContextoPermisos.Crear();
        const int administradorId = 121;
        const string tipoPermiso = "rf-gp-07-acceso-reportes";
        await contexto.Servicio.CrearAsync(administradorId, tipoPermiso, CancellationToken.None);
        var solicitudId = contexto.Solicitudes.ObtenerTodas()[0].Id;

        // Actuar: intenta aprobar su propia solicitud.
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() =>
            contexto.Servicio.AprobarAsync(solicitudId, administradorId, CancellationToken.None));

        // Verificar: error de negocio con codigo 409 y solicitud sin resolver.
        Assert.Equal("No puedes resolver tu propia solicitud.", excepcion.Message);
        Assert.Equal(409, excepcion.CodigoHttp);

        var solicitudFinal = await contexto.Solicitudes.BuscarPorIdAsync(solicitudId);
        Assert.NotNull(solicitudFinal);
        Assert.Equal(EstadoSolicitudPermiso.Pendiente, solicitudFinal.Estado);
        Assert.Null(solicitudFinal.ResueltaPorId);
        Assert.Null(solicitudFinal.FechaResolucion);
        Assert.Empty(contexto.PermisosActivos.ObtenerTodos());
    }

    [Fact]
    public async Task RF_GP_07_Rechazar_PropiaSolicitud_LanzaReglaNegocio409YEstadoNoCambia()
    {
        // Preparar: el Administrador crea su propia solicitud y un motivo valido.
        var contexto = ContextoPermisos.Crear();
        const int administradorId = 122;
        const string tipoPermiso = "rf-gp-07-acceso-calificaciones";
        await contexto.Servicio.CrearAsync(administradorId, tipoPermiso, CancellationToken.None);
        var solicitudId = contexto.Solicitudes.ObtenerTodas()[0].Id;

        // Actuar: intenta rechazar su propia solicitud.
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() =>
            contexto.Servicio.RechazarAsync(solicitudId, administradorId, "Motivo valido.", CancellationToken.None));

        // Verificar: error de negocio con codigo 409 y solicitud sin resolver.
        Assert.Equal("No puedes resolver tu propia solicitud.", excepcion.Message);
        Assert.Equal(409, excepcion.CodigoHttp);

        var solicitudFinal = await contexto.Solicitudes.BuscarPorIdAsync(solicitudId);
        Assert.NotNull(solicitudFinal);
        Assert.Equal(EstadoSolicitudPermiso.Pendiente, solicitudFinal.Estado);
        Assert.Null(solicitudFinal.ResueltaPorId);
        Assert.Null(solicitudFinal.FechaResolucion);
        Assert.Null(solicitudFinal.MotivoRechazo);
    }
}
