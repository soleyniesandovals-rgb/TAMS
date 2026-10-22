using Core.Permisos.Domain.Enums;
using Tests.Permisos.Dobles;
using Xunit;

namespace Tests.Permisos;

/// <summary>
/// RF-GP-01: crear solicitud de permiso.
/// La solicitud nace en estado Pendiente y espera a que un Administrador la resuelva.
/// </summary>
public class CrearSolicitudTests
{
    [Fact]
    public async Task RF_GP_01_Crear_Solicitud_NacePendiente()
    {
        // Preparar: un solicitante y un tipo de permiso validos.
        var contexto = ContextoPermisos.Crear();
        const int solicitanteId = 41;
        const string tipoPermiso = "rf-gp-01-acceso-reportes";

        // Actuar: el solicitante crea su solicitud.
        var resultado = await contexto.Servicio.CrearAsync(solicitanteId, tipoPermiso, CancellationToken.None);

        // Verificar: nace Pendiente, sin resolver y sin motivo de rechazo.
        Assert.Equal(solicitanteId, resultado.SolicitanteId);
        Assert.Equal(tipoPermiso, resultado.TipoPermiso);
        Assert.Equal(EstadoSolicitudPermiso.Pendiente.ToString(), resultado.Estado);
        Assert.Null(resultado.ResueltaPorId);
        Assert.Null(resultado.FechaResolucion);
        Assert.Null(resultado.MotivoRechazo);
    }
}
