using Core.Auditoria.Domain.Enums;
using Core.ControlAcceso.Domain.Excepciones;
using Core.Permisos.Domain.Enums;
using Tests.Permisos.Dobles;
using Xunit;

namespace Tests.Permisos;

/// <summary>
/// RF-AUD-02: auditoría de la resolución de solicitudes de permiso (CA-02, CA-03,
/// CA-12 y CA-13). El registrador es el real, sobre la unidad de trabajo falsa.
/// </summary>
public class ResolucionSolicitudesAuditoriaTests
{
    [Fact]
    public async Task RF_AUD_02_AprobarAsync_RegistraLosSeisCampos()
    {
        // CA-02: AprobarSolicitud / SolicitudPermiso / Pendiente -> Aplicada (<tipo>).
        // Preparar: una solicitud pendiente de otro usuario y un administrador.
        var (servicio, unidad, solicitudes, _, _) = ContextoPermisos.Crear();
        var solicitud = await ContextoPermisos.SembrarAsync(
            solicitudes, 5, "Vacaciones", EstadoSolicitudPermiso.Pendiente);

        // Actuar: el administrador aprueba la solicitud.
        await servicio.AprobarAsync(solicitud.Id, 3, CancellationToken.None);

        // Verificar: exactamente un registro con los seis campos exigidos.
        var registro = Assert.Single(unidad.Registros);
        Assert.Equal(3, registro.UsuarioId);
        Assert.Equal(AccionAuditoria.AprobarSolicitud, registro.Accion);
        Assert.Equal(EntidadAuditoria.SolicitudPermiso, registro.Entidad);
        Assert.Equal(solicitud.Id, registro.EntidadId);
        Assert.Equal("Pendiente", registro.ValorAnterior);
        Assert.Equal("Aplicada (Vacaciones)", registro.ValorNuevo);
    }

    [Fact]
    public async Task RF_AUD_02_RechazarAsync_RegistraElMotivoGuardado()
    {
        // CA-03: RechazarSolicitud con el mismo motivo persistido en la solicitud.
        // Preparar: una solicitud pendiente y un motivo con espacios que deben recortarse.
        var (servicio, unidad, solicitudes, _, _) = ContextoPermisos.Crear();
        var solicitud = await ContextoPermisos.SembrarAsync(
            solicitudes, 5, "Permiso médico", EstadoSolicitudPermiso.Pendiente);

        // Actuar: se rechaza con motivo; el servicio guarda el motivo recortado.
        await servicio.RechazarAsync(solicitud.Id, 3, "  Documentación incompleta  ", CancellationToken.None);

        // Verificar: el registro usa exactamente el motivo guardado, sin espacios sobrantes.
        var registro = Assert.Single(unidad.Registros);
        Assert.Equal(3, registro.UsuarioId);
        Assert.Equal(AccionAuditoria.RechazarSolicitud, registro.Accion);
        Assert.Equal(EntidadAuditoria.SolicitudPermiso, registro.Entidad);
        Assert.Equal(solicitud.Id, registro.EntidadId);
        Assert.Equal("Pendiente", registro.ValorAnterior);
        Assert.Equal($"Rechazada: {solicitud.MotivoRechazo}", registro.ValorNuevo);
        Assert.Equal("Rechazada: Documentación incompleta", registro.ValorNuevo);
    }

    [Fact]
    public async Task RF_AUD_02_AprobarAsync_AutoResolucionRechazada_NoRegistra()
    {
        // CA-13: una operación rechazada con 409 no genera ningún registro.
        // Preparar: el administrador es el propio solicitante.
        var (servicio, unidad, solicitudes, _, _) = ContextoPermisos.Crear();
        var solicitud = await ContextoPermisos.SembrarAsync(
            solicitudes, 3, "Vacaciones", EstadoSolicitudPermiso.Pendiente);

        // Actuar: intenta aprobar su propia solicitud.
        await Assert.ThrowsAsync<ReglaNegocioExcepcion>(
            () => servicio.AprobarAsync(solicitud.Id, 3, CancellationToken.None));

        // Verificar: ni registro ni guardado.
        Assert.Empty(unidad.Registros);
        Assert.Equal(0, unidad.Guardados);
    }

    [Fact]
    public async Task RF_AUD_02_RechazarAsync_SinMotivoRechazado_NoRegistra()
    {
        // CA-13: una operación rechazada con 400 no genera ningún registro.
        // Preparar: una solicitud pendiente ajena.
        var (servicio, unidad, solicitudes, _, _) = ContextoPermisos.Crear();
        var solicitud = await ContextoPermisos.SembrarAsync(
            solicitudes, 5, "Vacaciones", EstadoSolicitudPermiso.Pendiente);

        // Actuar: intenta rechazar sin motivo.
        await Assert.ThrowsAsync<ReglaNegocioExcepcion>(
            () => servicio.RechazarAsync(solicitud.Id, 3, "   ", CancellationToken.None));

        // Verificar: ni registro ni guardado.
        Assert.Empty(unidad.Registros);
        Assert.Equal(0, unidad.Guardados);
    }

    [Fact]
    public async Task RF_AUD_02_AprobarAsync_RegistroSeGuardaConLaOperacion()
    {
        // CA-12: el registro entra en el único guardado de la operación; el registrador no guarda.
        // Preparar: una solicitud pendiente ajena.
        var (servicio, unidad, solicitudes, _, _) = ContextoPermisos.Crear();
        var solicitud = await ContextoPermisos.SembrarAsync(
            solicitudes, 5, "Estudio", EstadoSolicitudPermiso.Pendiente);

        // Actuar: una sola aprobación.
        await servicio.AprobarAsync(solicitud.Id, 3, CancellationToken.None);

        // Verificar: un solo guardado y el registro ya estaba presente en ese momento.
        Assert.Equal(1, unidad.Guardados);
        Assert.Equal(new[] { 1 }, unidad.RegistrosPorGuardado);
        Assert.Single(unidad.Registros);
    }
}
