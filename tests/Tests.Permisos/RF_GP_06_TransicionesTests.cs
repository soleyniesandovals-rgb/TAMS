using Core.Permisos.Domain.Enums;
using Core.Permisos.Domain.Excepciones;
using Core.Permisos.Domain.Maquinas;
using Tests.Permisos.Dobles;
using Xunit;

namespace Tests.Permisos;

/// <summary>
/// RF-GP-06: transiciones validas unicas (maquina de estados).
/// Una solicitud ya resuelta no se vuelve a resolver: solo Pendiente puede pasar a Aprobada
/// o a Rechazada, y Rechazada y Aplicada son estados terminales.
/// </summary>
public class TransicionesTests
{
    [Fact]
    public async Task RF_GP_06_Rechazar_DesdeRechazada_TransicionProhibidaYEstadoNoCambia()
    {
        // Preparar: una solicitud ya Rechazada.
        var contexto = ContextoPermisos.Crear();
        const int solicitanteId = 101;
        const int administradorId = 102;
        var solicitud = await ContextoPermisos.SembrarAsync(
            contexto.Solicitudes, solicitanteId, "rf-gp-06-rechazada", EstadoSolicitudPermiso.Rechazada);

        // Actuar: el Administrador intenta rechazarla otra vez.
        var excepcion = await Assert.ThrowsAsync<TransicionNoPermitidaExcepcion>(() =>
            contexto.Servicio.RechazarAsync(solicitud.Id, administradorId, "Motivo valido.", CancellationToken.None));

        // Verificar: transicion prohibida y estado intacto.
        Assert.Equal(EstadoSolicitudPermiso.Rechazada, excepcion.Origen);
        Assert.Equal(EstadoSolicitudPermiso.Rechazada, excepcion.Destino);

        var solicitudFinal = await contexto.Solicitudes.BuscarPorIdAsync(solicitud.Id);
        Assert.NotNull(solicitudFinal);
        Assert.Equal(EstadoSolicitudPermiso.Rechazada, solicitudFinal.Estado);
    }

    [Fact]
    public async Task RF_GP_06_Rechazar_DesdeAprobada_TransicionProhibidaYEstadoNoCambia()
    {
        // Preparar: una solicitud en estado Aprobada (sembrada directamente).
        var contexto = ContextoPermisos.Crear();
        const int solicitanteId = 103;
        const int administradorId = 104;
        var solicitud = await ContextoPermisos.SembrarAsync(
            contexto.Solicitudes, solicitanteId, "rf-gp-06-aprobada-a-rechazar", EstadoSolicitudPermiso.Aprobada);

        // Actuar: el Administrador intenta rechazarla.
        var excepcion = await Assert.ThrowsAsync<TransicionNoPermitidaExcepcion>(() =>
            contexto.Servicio.RechazarAsync(solicitud.Id, administradorId, "Motivo valido.", CancellationToken.None));

        // Verificar: transicion prohibida y estado intacto.
        Assert.Equal(EstadoSolicitudPermiso.Aprobada, excepcion.Origen);
        Assert.Equal(EstadoSolicitudPermiso.Rechazada, excepcion.Destino);

        var solicitudFinal = await contexto.Solicitudes.BuscarPorIdAsync(solicitud.Id);
        Assert.NotNull(solicitudFinal);
        Assert.Equal(EstadoSolicitudPermiso.Aprobada, solicitudFinal.Estado);
    }

    [Fact]
    public async Task RF_GP_06_Rechazar_DesdeAplicada_TransicionProhibidaYEstadoNoCambia()
    {
        // Preparar: una solicitud en estado Aplicada (sembrada directamente).
        var contexto = ContextoPermisos.Crear();
        const int solicitanteId = 105;
        const int administradorId = 106;
        var solicitud = await ContextoPermisos.SembrarAsync(
            contexto.Solicitudes, solicitanteId, "rf-gp-06-aplicada-a-rechazar", EstadoSolicitudPermiso.Aplicada);

        // Actuar: el Administrador intenta rechazarla.
        var excepcion = await Assert.ThrowsAsync<TransicionNoPermitidaExcepcion>(() =>
            contexto.Servicio.RechazarAsync(solicitud.Id, administradorId, "Motivo valido.", CancellationToken.None));

        // Verificar: transicion prohibida y estado intacto.
        Assert.Equal(EstadoSolicitudPermiso.Aplicada, excepcion.Origen);
        Assert.Equal(EstadoSolicitudPermiso.Rechazada, excepcion.Destino);

        var solicitudFinal = await contexto.Solicitudes.BuscarPorIdAsync(solicitud.Id);
        Assert.NotNull(solicitudFinal);
        Assert.Equal(EstadoSolicitudPermiso.Aplicada, solicitudFinal.Estado);
    }

    [Fact]
    public async Task RF_GP_06_Aprobar_DesdeAprobada_TransicionProhibidaYEstadoNoCambia()
    {
        // Preparar: una solicitud en estado Aprobada (sembrada directamente).
        var contexto = ContextoPermisos.Crear();
        const int solicitanteId = 107;
        const int administradorId = 108;
        var solicitud = await ContextoPermisos.SembrarAsync(
            contexto.Solicitudes, solicitanteId, "rf-gp-06-aprobada-a-aprobar", EstadoSolicitudPermiso.Aprobada);

        // Actuar: el Administrador intenta aprobarla.
        var excepcion = await Assert.ThrowsAsync<TransicionNoPermitidaExcepcion>(() =>
            contexto.Servicio.AprobarAsync(solicitud.Id, administradorId, CancellationToken.None));

        // Verificar: transicion prohibida y estado intacto.
        Assert.Equal(EstadoSolicitudPermiso.Aprobada, excepcion.Origen);
        Assert.Equal(EstadoSolicitudPermiso.Aprobada, excepcion.Destino);

        var solicitudFinal = await contexto.Solicitudes.BuscarPorIdAsync(solicitud.Id);
        Assert.NotNull(solicitudFinal);
        Assert.Equal(EstadoSolicitudPermiso.Aprobada, solicitudFinal.Estado);
    }

    [Fact]
    public async Task RF_GP_06_Aprobar_DesdeAplicada_TransicionProhibidaYEstadoNoCambia()
    {
        // Preparar: una solicitud en estado Aplicada (sembrada directamente).
        var contexto = ContextoPermisos.Crear();
        const int solicitanteId = 109;
        const int administradorId = 110;
        var solicitud = await ContextoPermisos.SembrarAsync(
            contexto.Solicitudes, solicitanteId, "rf-gp-06-aplicada-a-aprobar", EstadoSolicitudPermiso.Aplicada);

        // Actuar: el Administrador intenta aprobarla otra vez.
        var excepcion = await Assert.ThrowsAsync<TransicionNoPermitidaExcepcion>(() =>
            contexto.Servicio.AprobarAsync(solicitud.Id, administradorId, CancellationToken.None));

        // Verificar: transicion prohibida y estado intacto.
        Assert.Equal(EstadoSolicitudPermiso.Aplicada, excepcion.Origen);
        Assert.Equal(EstadoSolicitudPermiso.Aprobada, excepcion.Destino);

        var solicitudFinal = await contexto.Solicitudes.BuscarPorIdAsync(solicitud.Id);
        Assert.NotNull(solicitudFinal);
        Assert.Equal(EstadoSolicitudPermiso.Aplicada, solicitudFinal.Estado);
    }

    [Fact]
    public async Task RF_GP_06_Aprobar_DesdeRechazada_TransicionProhibidaYEstadoNoCambia()
    {
        // Preparar: una solicitud ya Rechazada.
        var contexto = ContextoPermisos.Crear();
        const int solicitanteId = 111;
        const int administradorId = 112;
        var solicitud = await ContextoPermisos.SembrarAsync(
            contexto.Solicitudes, solicitanteId, "rf-gp-06-rechazada-a-aprobar", EstadoSolicitudPermiso.Rechazada);

        // Actuar: el Administrador intenta aprobarla.
        var excepcion = await Assert.ThrowsAsync<TransicionNoPermitidaExcepcion>(() =>
            contexto.Servicio.AprobarAsync(solicitud.Id, administradorId, CancellationToken.None));

        // Verificar: transicion prohibida y estado intacto.
        Assert.Equal(EstadoSolicitudPermiso.Rechazada, excepcion.Origen);
        Assert.Equal(EstadoSolicitudPermiso.Aprobada, excepcion.Destino);

        var solicitudFinal = await contexto.Solicitudes.BuscarPorIdAsync(solicitud.Id);
        Assert.NotNull(solicitudFinal);
        Assert.Equal(EstadoSolicitudPermiso.Rechazada, solicitudFinal.Estado);
    }

    [Fact]
    public void RF_GP_06_Tabla_Rechazada_NoPermitePasarAAplicada()
    {
        // Actuar: se consulta la tabla de transiciones.
        var esValida = TransicionesPermiso.EsValida(EstadoSolicitudPermiso.Rechazada, EstadoSolicitudPermiso.Aplicada);
        var destinos = TransicionesPermiso.PermitidosDesde(EstadoSolicitudPermiso.Rechazada);

        // Verificar: la transicion no existe y Rechazada es terminal.
        Assert.False(esValida);
        Assert.Empty(destinos);
        Assert.True(TransicionesPermiso.EsTerminal(EstadoSolicitudPermiso.Rechazada));
    }

    [Fact]
    public void RF_GP_06_Tabla_Aplicada_NoPermitePasarAAprobada()
    {
        // Actuar: se consulta la tabla de transiciones.
        var esValida = TransicionesPermiso.EsValida(EstadoSolicitudPermiso.Aplicada, EstadoSolicitudPermiso.Aprobada);
        var destinos = TransicionesPermiso.PermitidosDesde(EstadoSolicitudPermiso.Aplicada);

        // Verificar: la transicion no existe y Aplicada es terminal.
        Assert.False(esValida);
        Assert.Empty(destinos);
        Assert.True(TransicionesPermiso.EsTerminal(EstadoSolicitudPermiso.Aplicada));
    }
}
