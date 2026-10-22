using Core.Permisos.Domain.Enums;
using Tests.Permisos.Dobles;
using Xunit;

namespace Tests.Permisos;

/// <summary>
/// RF-GP-02: listado de solicitudes.
/// El Administrador recibe solo las solicitudes en estado Pendiente, y cada usuario
/// autenticado ve solo las solicitudes propias.
/// </summary>
public class ListarSolicitudesTests
{
    [Fact]
    public async Task RF_GP_02_ListarPendientes_Administrador_SoloRecibeLasPendientes()
    {
        // Preparar: una solicitud en cada estado, con datos distintos entre si.
        var contexto = ContextoPermisos.Crear();
        await ContextoPermisos.SembrarAsync(contexto.Solicitudes, 51, "rf-gp-02-pendiente", EstadoSolicitudPermiso.Pendiente);
        await ContextoPermisos.SembrarAsync(contexto.Solicitudes, 52, "rf-gp-02-aprobada", EstadoSolicitudPermiso.Aprobada);
        await ContextoPermisos.SembrarAsync(contexto.Solicitudes, 53, "rf-gp-02-aplicada", EstadoSolicitudPermiso.Aplicada);
        await ContextoPermisos.SembrarAsync(contexto.Solicitudes, 54, "rf-gp-02-rechazada", EstadoSolicitudPermiso.Rechazada);

        // Actuar: el Administrador pide la bandeja de pendientes.
        var pendientes = await contexto.Servicio.ListarPendientesAsync(CancellationToken.None);

        // Verificar: solo entra la solicitud que sigue en Pendiente.
        Assert.Single(pendientes);
        Assert.Equal("rf-gp-02-pendiente", pendientes[0].TipoPermiso);
        Assert.Equal(EstadoSolicitudPermiso.Pendiente.ToString(), pendientes[0].Estado);
    }

    [Fact]
    public async Task RF_GP_02_ListarPropias_Estandar_NoVeSolicitudesDeOtroUsuario()
    {
        // Preparar: dos usuarios, cada uno con sus propias solicitudes.
        var contexto = ContextoPermisos.Crear();
        const int usuarioEstandar = 61;
        const int otroUsuario = 62;
        await ContextoPermisos.SembrarAsync(contexto.Solicitudes, usuarioEstandar, "rf-gp-02-propia-1", EstadoSolicitudPermiso.Pendiente);
        await ContextoPermisos.SembrarAsync(contexto.Solicitudes, usuarioEstandar, "rf-gp-02-propia-2", EstadoSolicitudPermiso.Aprobada);
        await ContextoPermisos.SembrarAsync(contexto.Solicitudes, otroUsuario, "rf-gp-02-ajena", EstadoSolicitudPermiso.Pendiente);

        // Actuar: cada usuario lista sus propias solicitudes.
        var propiasDelEstandar = await contexto.Servicio.ListarPropiasAsync(usuarioEstandar, CancellationToken.None);
        var propiasDelOtro = await contexto.Servicio.ListarPropiasAsync(otroUsuario, CancellationToken.None);

        // Verificar: el Estandar no ve nada del otro usuario.
        Assert.Equal(2, propiasDelEstandar.Count);
        Assert.All(propiasDelEstandar, p => Assert.Equal(usuarioEstandar, p.SolicitanteId));
        Assert.DoesNotContain(propiasDelEstandar, p => p.TipoPermiso == "rf-gp-02-ajena");

        Assert.Single(propiasDelOtro);
        Assert.Equal("rf-gp-02-ajena", propiasDelOtro[0].TipoPermiso);
    }
}
