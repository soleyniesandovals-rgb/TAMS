using Core.ControlAcceso.Domain.Excepciones;
using Core.Permisos.Domain.Enums;
using Tests.Permisos.Dobles;
using Xunit;

namespace Tests.Permisos;

/// <summary>
/// RF-GP-04: rechazar una solicitud.
/// El motivo es obligatorio: sin motivo valido el rechazo no se completa y la solicitud
/// conserva su estado. Con motivo valido, la solicitud queda Rechazada y el motivo guardado.
/// </summary>
public class RechazarSolicitudTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task RF_GP_04_Rechazar_SinMotivo_NoCompletaYEstadoNoCambia(string? motivo)
    {
        // Preparar: una solicitud Pendiente de otro usuario y un Administrador.
        var contexto = ContextoPermisos.Crear();
        const int solicitanteId = 81;
        const int administradorId = 82;
        const string tipoPermiso = "rf-gp-04-acceso-reportes";
        await contexto.Servicio.CrearAsync(solicitanteId, tipoPermiso, CancellationToken.None);
        var solicitudId = contexto.Solicitudes.ObtenerTodas()[0].Id;

        // Actuar: el Administrador intenta rechazar sin motivo util.
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() =>
            contexto.Servicio.RechazarAsync(solicitudId, administradorId, motivo!, CancellationToken.None));

        // Verificar: error de negocio controlado y la solicitud sin tocar.
        Assert.Equal("El rechazo requiere un motivo.", excepcion.Message);
        Assert.Equal(400, excepcion.CodigoHttp);

        var solicitudFinal = await contexto.Solicitudes.BuscarPorIdAsync(solicitudId);
        Assert.NotNull(solicitudFinal);
        Assert.Equal(EstadoSolicitudPermiso.Pendiente, solicitudFinal.Estado);
        Assert.Null(solicitudFinal.ResueltaPorId);
        Assert.Null(solicitudFinal.FechaResolucion);
        Assert.Null(solicitudFinal.MotivoRechazo);
    }

    [Fact]
    public async Task RF_GP_04_Rechazar_ConMotivo_CompletaYGuardaMotivo()
    {
        // Preparar: una solicitud Pendiente de otro usuario, un Administrador y un motivo.
        var contexto = ContextoPermisos.Crear();
        const int solicitanteId = 83;
        const int administradorId = 84;
        const string tipoPermiso = "rf-gp-04-acceso-calificaciones";
        const string motivo = "No corresponde al cargo.";
        await contexto.Servicio.CrearAsync(solicitanteId, tipoPermiso, CancellationToken.None);
        var solicitudId = contexto.Solicitudes.ObtenerTodas()[0].Id;

        // Actuar: el Administrador rechaza con motivo valido.
        var resultado = await contexto.Servicio.RechazarAsync(solicitudId, administradorId, motivo, CancellationToken.None);

        // Verificar: la solicitud queda Rechazada, con el motivo guardado y la trazabilidad.
        Assert.Equal(EstadoSolicitudPermiso.Rechazada.ToString(), resultado.Estado);
        Assert.Equal(motivo, resultado.MotivoRechazo);
        Assert.Equal(administradorId, resultado.ResueltaPorId);
        Assert.NotNull(resultado.FechaResolucion);
    }
}
