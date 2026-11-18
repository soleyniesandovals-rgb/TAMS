using Core.ControlAcceso.Application.Interfaces;
using Core.ControlAcceso.Application.Opciones;
using Core.ControlAcceso.Application.Servicios;
using Core.ControlAcceso.Domain.Entidades;
using Core.ControlAcceso.Domain.Enums;
using Core.Notificaciones.Application.Servicios;
using Core.Notificaciones.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Tests.Core.Dobles;
using Xunit;

namespace Tests.Core;

/// <summary>
/// C-09 a C-11 (RF-NOT-06, RF-NOT-07): correo asociado a las notificaciones, preferencia
/// y correos de cuenta no afectados.
/// </summary>
public class NotificadorPreferenciasTests
{
    private const string Titulo = "Nueva solicitud de permiso";
    private const string Mensaje = "Ana solicitó acceso-reportes.";

    [Fact]
    public async Task RF_NOT_06_AgregarSinPreferencia_EncolaCorreoPendienteConTituloYMensaje()
    {
        // Preparar: usuario sin fila de preferencia (correo activo por defecto).
        var contexto = ContextoNotificaciones.Crear();
        await ContextoNotificaciones.SembrarUsuarioAsync(
            contexto.Contexto, 1, "Ana", "ana@ejemplo.com", RolUsuario.Administrador);

        // Actuar: se añade una notificación y se guarda una sola vez.
        await contexto.Notificador.AgregarAsync(
            1, TipoNotificacion.SolicitudCreada, Titulo, Mensaje, CancellationToken.None);
        await contexto.Unidad.GuardarCambiosAsync(CancellationToken.None);

        // Verificar: bandeja y cola; el correo usa el título como asunto y el mensaje como cuerpo.
        Assert.Single(await contexto.Contexto.Notificaciones.AsNoTracking().ToListAsync());
        var correo = Assert.Single(await contexto.Contexto.CorreosEnCola.AsNoTracking().ToListAsync());
        Assert.Equal("ana@ejemplo.com", correo.Destinatario);
        Assert.Equal(Titulo, correo.Asunto);
        Assert.Equal(Mensaje, correo.Cuerpo);
        Assert.Equal(EstadoCorreo.Pendiente, correo.Estado);
    }

    [Fact]
    public async Task RF_NOT_07_ConCorreoDesactivado_NoEncolaYAlReactivarVuelveAEncolar()
    {
        // Preparar: usuario con la preferencia de correo desactivada.
        var contexto = ContextoNotificaciones.Crear();
        await ContextoNotificaciones.SembrarUsuarioAsync(
            contexto.Contexto, 1, "Ana", "ana@ejemplo.com", RolUsuario.Administrador);
        await contexto.Servicio.CambiarPreferenciaAsync(1, false, CancellationToken.None);

        // Actuar: primera notificación con correo desactivado; segunda tras reactivarlo.
        await contexto.Notificador.AgregarAsync(
            1, TipoNotificacion.SolicitudCreada, Titulo, Mensaje, CancellationToken.None);
        await contexto.Unidad.GuardarCambiosAsync(CancellationToken.None);
        var sinCorreo = await contexto.Contexto.CorreosEnCola.AsNoTracking().CountAsync();

        await contexto.Servicio.CambiarPreferenciaAsync(1, true, CancellationToken.None);
        Assert.True(await contexto.Servicio.ObtenerPreferenciaAsync(1, CancellationToken.None));
        await contexto.Notificador.AgregarAsync(
            1, TipoNotificacion.SolicitudAprobada, Titulo, Mensaje, CancellationToken.None);
        await contexto.Unidad.GuardarCambiosAsync(CancellationToken.None);

        // Verificar: la bandeja tiene ambas; la cola solo el correo posterior a la reactivación.
        Assert.Equal(2, await contexto.Contexto.Notificaciones.AsNoTracking().CountAsync());
        Assert.Equal(0, sinCorreo);
        Assert.Equal(1, await contexto.Contexto.CorreosEnCola.AsNoTracking().CountAsync());
    }

    [Fact]
    public async Task RF_NOT_07_PreferenciaDesactivada_NoAfectaCorreoDeRecuperacion()
    {
        // Preparar: unidad en memoria con la preferencia de correo desactivada.
        var reloj = new TimeProviderFijo(new DateTimeOffset(2026, 10, 17, 12, 0, 0, TimeSpan.Zero));
        var unidad = new CoreUnidadDeTrabajoFalso(reloj);
        await unidad.Usuarios.AgregarAsync(new Usuario
        {
            Id = 1,
            Nombre = "Ana",
            Correo = "ana@ejemplo.com",
            ContraseñaHash = "hash-de-prueba",
            Rol = RolUsuario.Administrador,
            Activo = true,
        });
        var servicioNotificaciones = new NotificacionesServicio(unidad, reloj);
        await servicioNotificaciones.CambiarPreferenciaAsync(1, false, CancellationToken.None);
        var recuperacion = new RecuperacionContrasenaServicio(
            unidad,
            new TokenGeneradorFalso(),
            new ContrasenaHasherFalso(),
            reloj,
            Options.Create(new OpcionesRecuperacionContrasena
            {
                UrlRecuperacionPlantilla = "https://ejemplo.invalid/recuperar?codigo={codigo}",
                HorasVencimientoCodigo = 1,
            }));

        // Actuar: una notificación con preferencia apagada y una recuperación de contraseña.
        await new Notificador(unidad).NotificarAsync(
            1, TipoNotificacion.SolicitudCreada, Titulo, Mensaje, CancellationToken.None);
        await recuperacion.IniciarAsync("ana@ejemplo.com", CancellationToken.None);

        // Verificar: la notificación no encoló correo, pero la recuperación sí lo hizo.
        Assert.Single(unidad.NotificacionesGuardadas);
        var correo = Assert.Single(unidad.CorreosGuardados);
        Assert.Equal("ana@ejemplo.com", correo.Destinatario);
        Assert.Equal("Recupera tu contraseña en TAMS", correo.Asunto);
        Assert.Equal(EstadoCorreo.Pendiente, correo.Estado);
    }

    private sealed class TokenGeneradorFalso : ITokenGenerador
    {
        public string Generar() => "codigo-de-prueba";
    }

    private sealed class ContrasenaHasherFalso : IContrasenaHasher
    {
        public string Hash(string contrasena) => $"hash-de-{contrasena}";

        public bool Verificar(string contrasena, string hash) => hash == Hash(contrasena);
    }
}
