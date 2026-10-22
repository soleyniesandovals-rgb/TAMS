using Core.Permisos.Domain.Enums;
using Tests.Permisos.Dobles;
using Xunit;

namespace Tests.Permisos;

/// <summary>
/// RF-GP-05: la aprobación aplica el permiso automáticamente.
/// Antes de aprobar, el solicitante no tiene el permiso; después de aprobar, lo tiene y la
/// solicitud queda en estado Aplicada.
/// </summary>
public class AplicarPermisoTests
{
    [Fact]
    public async Task RF_GP_05_SinAprobar_ElSolicitante_NoTieneElPermiso()
    {
        // Preparar: una solicitud Pendiente de un tipo de permiso concreto.
        var contexto = ContextoPermisos.Crear();
        const int solicitanteId = 91;
        const string tipoPermiso = "rf-gp-05-acceso-reportes";
        await contexto.Servicio.CrearAsync(solicitanteId, tipoPermiso, CancellationToken.None);

        // Actuar: se consulta el acceso sin que nadie haya aprobado la solicitud.
        var tienePermiso = await contexto.Servicio.TienePermisoAsync(solicitanteId, tipoPermiso, CancellationToken.None);

        // Verificar: todavia no tiene el permiso.
        Assert.False(tienePermiso);
        Assert.Empty(contexto.PermisosActivos.ObtenerTodos());
    }

    [Fact]
    public async Task RF_GP_05_Aprobar_AplicaElPermisoYDejaLaSolicitudAplicada()
    {
        // Preparar: una solicitud Pendiente de un solicitante, y un Administrador distinto.
        var contexto = ContextoPermisos.Crear();
        const int solicitanteId = 92;
        const int administradorId = 93;
        const string tipoPermiso = "rf-gp-05-acceso-calificaciones";
        await contexto.Servicio.CrearAsync(solicitanteId, tipoPermiso, CancellationToken.None);
        var solicitudId = contexto.Solicitudes.ObtenerTodas()[0].Id;

        // Actuar: el Administrador aprueba la solicitud.
        var resultado = await contexto.Servicio.AprobarAsync(solicitudId, administradorId, CancellationToken.None);

        // Verificar: el permiso queda otorgado y la solicitud, en estado Aplicada.
        var tienePermiso = await contexto.Servicio.TienePermisoAsync(solicitanteId, tipoPermiso, CancellationToken.None);
        Assert.True(tienePermiso);
        Assert.Equal(EstadoSolicitudPermiso.Aplicada.ToString(), resultado.Estado);

        var permisosActivos = contexto.PermisosActivos.ObtenerTodos();
        Assert.Single(permisosActivos);
        Assert.Equal(solicitanteId, permisosActivos[0].UsuarioId);
        Assert.Equal(tipoPermiso, permisosActivos[0].TipoPermiso);
        Assert.Equal(solicitudId, permisosActivos[0].SolicitudPermisoId);
    }
}
