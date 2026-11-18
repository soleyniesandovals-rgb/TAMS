using Core.ControlAcceso.Domain.Entidades;
using Core.ControlAcceso.Domain.Enums;
using Core.Notificaciones.Domain.Enums;
using Core.Permisos.Domain.Enums;
using Tests.Permisos.Dobles;
using Xunit;

namespace Tests.Permisos;

/// <summary>
/// C-01 a C-03 (RF-NOT-01, RF-NOT-02): eventos de notificación en la gestión de permisos.
/// </summary>
public class EventosSolicitudNotificacionTests
{
    [Fact]
    public async Task RF_NOT_01_CrearSolicitud_NotificaSoloAAdministradoresActivos()
    {
        // Preparar: dos Administradores activos, uno desactivado y dos Estándar.
        var administradores = new[]
        {
            Usuario(1, "Admin Uno", "admin1@ejemplo.com", RolUsuario.Administrador, true),
            Usuario(2, "Admin Dos", "admin2@ejemplo.com", RolUsuario.Administrador, true),
            Usuario(3, "Admin Inactivo", "admin3@ejemplo.com", RolUsuario.Administrador, false),
            Usuario(4, "Solicitante", "solicitante@ejemplo.com", RolUsuario.Estandar, true),
            Usuario(5, "Otro Estandar", "otro@ejemplo.com", RolUsuario.Estandar, true),
        };
        var contexto = ContextoPermisos.Crear(
            TimeProviderFijo.CrearUtc(2026, 10, 17, 12, 0, 0),
            new UsuariosRepositorioFalso(administradores));
        const string tipoPermiso = "acceso-reportes";

        // Actuar: el Estándar crea una solicitud.
        await contexto.Servicio.CrearAsync(4, tipoPermiso, CancellationToken.None);

        // Verificar: exactamente una notificación por Administrador activo, con tipo y solicitante.
        Assert.Equal(2, contexto.Notificador.Llamadas.Count);
        Assert.All(
            contexto.Notificador.Llamadas,
            llamada =>
            {
                Assert.Equal(TipoNotificacion.SolicitudCreada, llamada.Tipo);
                Assert.Contains(llamada.UsuarioId, new[] { 1, 2 });
                Assert.Contains(tipoPermiso, llamada.Mensaje, StringComparison.Ordinal);
                Assert.Contains("Solicitante", llamada.Mensaje, StringComparison.Ordinal);
                Assert.Contains("solicitante@ejemplo.com", llamada.Mensaje, StringComparison.Ordinal);
            });
    }

    [Fact]
    public async Task RF_NOT_01_CrearSolicitud_SinAdministradoresActivos_CreaIgualSinNotificar()
    {
        // Preparar: solo el usuario Estándar; no hay Administradores activos.
        var contexto = ContextoPermisos.Crear(
            TimeProviderFijo.CrearUtc(2026, 10, 17, 12, 0, 0),
            new UsuariosRepositorioFalso(
                Usuario(4, "Solicitante", "solicitante@ejemplo.com", RolUsuario.Estandar, true)));

        // Actuar: se crea la solicitud.
        var resultado = await contexto.Servicio.CrearAsync(4, "acceso-reportes", CancellationToken.None);

        // Verificar: la solicitud existe y no se añadió ninguna notificación.
        Assert.Equal(4, resultado.SolicitanteId);
        Assert.Equal(EstadoSolicitudPermiso.Pendiente.ToString(), resultado.Estado);
        Assert.Empty(contexto.Notificador.Llamadas);
    }

    [Fact]
    public async Task RF_NOT_02_AprobarSolicitud_NotificaAlSolicitanteConTipo()
    {
        // Preparar: una solicitud pendiente de un Estándar y un Administrador distinto.
        var contexto = ContextoPermisos.Crear(
            TimeProviderFijo.CrearUtc(2026, 10, 17, 12, 0, 0),
            new UsuariosRepositorioFalso(
                Usuario(1, "Admin", "admin@ejemplo.com", RolUsuario.Administrador, true),
                Usuario(4, "Solicitante", "solicitante@ejemplo.com", RolUsuario.Estandar, true)));
        const string tipoPermiso = "corregir-calificacion";
        var solicitud = await ContextoPermisos.SembrarAsync(
            contexto.Solicitudes, 4, tipoPermiso, EstadoSolicitudPermiso.Pendiente);

        // Actuar: el Administrador aprueba la solicitud.
        await contexto.Servicio.AprobarAsync(solicitud.Id, 1, CancellationToken.None);

        // Verificar: una notificación de aprobación al solicitante que nombra el tipo.
        var llamada = Assert.Single(contexto.Notificador.Llamadas);
        Assert.Equal(4, llamada.UsuarioId);
        Assert.Equal(TipoNotificacion.SolicitudAprobada, llamada.Tipo);
        Assert.Contains(tipoPermiso, llamada.Mensaje, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RF_NOT_02_RechazarSolicitud_NotificaAlSolicitanteConMotivoExacto()
    {
        // Preparar: una solicitud pendiente y un motivo de rechazo concreto.
        var contexto = ContextoPermisos.Crear(
            TimeProviderFijo.CrearUtc(2026, 10, 17, 12, 0, 0),
            new UsuariosRepositorioFalso(
                Usuario(1, "Admin", "admin@ejemplo.com", RolUsuario.Administrador, true),
                Usuario(4, "Solicitante", "solicitante@ejemplo.com", RolUsuario.Estandar, true)));
        const string tipoPermiso = "corregir-calificacion";
        const string motivo = "El motivo documentado no es suficiente.";
        var solicitud = await ContextoPermisos.SembrarAsync(
            contexto.Solicitudes, 4, tipoPermiso, EstadoSolicitudPermiso.Pendiente);

        // Actuar: el Administrador rechaza con ese motivo.
        await contexto.Servicio.RechazarAsync(solicitud.Id, 1, motivo, CancellationToken.None);

        // Verificar: el mensaje de la notificación contiene el motivo exacto.
        var llamada = Assert.Single(contexto.Notificador.Llamadas);
        Assert.Equal(4, llamada.UsuarioId);
        Assert.Equal(TipoNotificacion.SolicitudRechazada, llamada.Tipo);
        Assert.Contains(tipoPermiso, llamada.Mensaje, StringComparison.Ordinal);
        Assert.Contains(motivo, llamada.Mensaje, StringComparison.Ordinal);
    }

    private static Usuario Usuario(
        int id,
        string nombre,
        string correo,
        RolUsuario rol,
        bool activo)
        => new()
        {
            Id = id,
            Nombre = nombre,
            Correo = correo,
            ContraseñaHash = "hash-de-prueba",
            Rol = rol,
            Activo = activo,
        };
}
