using Core.ControlAcceso.Domain.Entidades;
using Core.ControlAcceso.Domain.Enums;
using Core.Notificaciones.Domain.Enums;
using Core.Permisos.Domain.Enums;
using Tests.Permisos.Dobles;
using Xunit;

namespace Tests.Permisos;

/// <summary>
/// A3-H3 (RF-NOT-01, RF-GP-07): el aviso de una solicitud nueva llega a los
/// Administradores activos, pero nunca al solicitante. Sin levantar la aplicación (RD-12).
/// </summary>
public class A3_H3_NotificarSolicitudCreadaTests
{
    [Fact]
    public async Task A3_H3_AdministradorSolicitante_NoSeNotificaASiMismoPeroSiAlOtroAdministrador()
    {
        // Preparar: dos Administradores activos; el Administrador 1 es el solicitante.
        var contexto = ContextoPermisos.Crear(
            TimeProviderFijo.CrearUtc(2026, 11, 18, 12, 0, 0),
            new UsuariosRepositorioFalso(
                Usuario(1, "Admin Solicitante", "admin1@ejemplo.com", RolUsuario.Administrador, true),
                Usuario(2, "Admin Receptor", "admin2@ejemplo.com", RolUsuario.Administrador, true)));
        const string tipoPermiso = "acceso-reportes";

        // Actuar: el propio Administrador 1 crea la solicitud.
        await contexto.Servicio.CrearAsync(1, tipoPermiso, CancellationToken.None);

        // Verificar: solo el otro Administrador recibe el aviso; el solicitante no.
        var llamada = Assert.Single(contexto.Notificador.Llamadas);
        Assert.Equal(2, llamada.UsuarioId);
        Assert.Equal(TipoNotificacion.SolicitudCreada, llamada.Tipo);
        Assert.Contains(tipoPermiso, llamada.Mensaje, StringComparison.Ordinal);
        Assert.DoesNotContain(
            contexto.Notificador.Llamadas,
            l => l.UsuarioId == 1);
    }

    [Fact]
    public async Task A3_H3_UnicoAdministradorEsElSolicitante_CreaLaSolicitudSinNotificar()
    {
        // Preparar: el único Administrador activo es el solicitante.
        var contexto = ContextoPermisos.Crear(
            TimeProviderFijo.CrearUtc(2026, 11, 18, 12, 0, 0),
            new UsuariosRepositorioFalso(
                Usuario(1, "Admin Solicitante", "admin1@ejemplo.com", RolUsuario.Administrador, true)));

        // Actuar: ese Administrador crea su propia solicitud.
        var resultado = await contexto.Servicio.CrearAsync(1, "acceso-reportes", CancellationToken.None);

        // Verificar: la solicitud se crea igual y nadie recibe una notificación.
        Assert.Equal(1, resultado.SolicitanteId);
        Assert.Equal(EstadoSolicitudPermiso.Pendiente.ToString(), resultado.Estado);
        Assert.Empty(contexto.Notificador.Llamadas);
    }

    [Fact]
    public async Task A3_H3_SolicitanteEstandar_SiguenNotificandoseLosDosAdministradores()
    {
        // Preparar: dos Administradores activos y un solicitante Estándar.
        var contexto = ContextoPermisos.Crear(
            TimeProviderFijo.CrearUtc(2026, 11, 18, 12, 0, 0),
            new UsuariosRepositorioFalso(
                Usuario(1, "Admin Uno", "admin1@ejemplo.com", RolUsuario.Administrador, true),
                Usuario(2, "Admin Dos", "admin2@ejemplo.com", RolUsuario.Administrador, true),
                Usuario(4, "Solicitante", "solicitante@ejemplo.com", RolUsuario.Estandar, true)));

        // Actuar: el Estándar crea la solicitud.
        await contexto.Servicio.CrearAsync(4, "acceso-reportes", CancellationToken.None);

        // Verificar: la exclusión del solicitante no quita a los Administradores (C-01).
        Assert.Equal(2, contexto.Notificador.Llamadas.Count);
        Assert.All(contexto.Notificador.Llamadas, l => Assert.Equal(TipoNotificacion.SolicitudCreada, l.Tipo));
        Assert.Equal([1, 2], contexto.Notificador.Llamadas.Select(l => l.UsuarioId).OrderBy(id => id).ToList());
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
