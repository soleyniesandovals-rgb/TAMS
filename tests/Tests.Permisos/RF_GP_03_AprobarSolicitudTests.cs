using Tests.Permisos.Dobles;
using Xunit;

namespace Tests.Permisos;

/// <summary>
/// RF-GP-03: la aprobación deja registrada la trazabilidad de la resolución.
/// Al aprobar, el servicio anota quién resolvió la solicitud y en qué instante (UTC).
/// </summary>
public class AprobarSolicitudTests
{
    [Fact]
    public async Task RF_GP_03_Aprobar_RegistraQuienResuelveYCuando()
    {
        // Preparar: un solicitante, un Administrador y un reloj fijo (17/10/2026 12:00:00 UTC).
        var contexto = ContextoPermisos.Crear(TimeProviderFijo.CrearUtc(2026, 10, 17, 12, 0, 0));
        const int solicitanteId = 71;
        const int administradorId = 72;
        const string tipoPermiso = "rf-gp-03-acceso-reportes";
        var instanteEsperado = new DateTime(2026, 10, 17, 12, 0, 0, DateTimeKind.Utc);
        await contexto.Servicio.CrearAsync(solicitanteId, tipoPermiso, CancellationToken.None);
        var solicitudId = contexto.Solicitudes.ObtenerTodas()[0].Id;

        // Actuar: el Administrador aprueba la solicitud.
        var resultado = await contexto.Servicio.AprobarAsync(solicitudId, administradorId, CancellationToken.None);

        // Verificar: queda registrado quien resolvio y el instante de la resolucion.
        Assert.Equal(administradorId, resultado.ResueltaPorId);
        Assert.Equal(instanteEsperado, resultado.FechaResolucion);
    }
}
