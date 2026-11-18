using Core.ControlAcceso.Domain.Entidades;
using Core.ControlAcceso.Domain.Enums;
using Core.ControlAcceso.Infrastructura.Persistence;
using Core.ControlAcceso.Infrastructura.Repositorios;
using Core.Notificaciones.Application.Servicios;
using Microsoft.EntityFrameworkCore;

namespace Tests.Core.Dobles;

/// <summary>
/// Contexto en memoria para las pruebas de notificaciones (RD-12): base única por prueba
/// y reloj controlado para verificar fechas UTC.
/// </summary>
public static class ContextoNotificaciones
{
    public static (
        TimeProviderFijo Reloj,
        CoreDbContext Contexto,
        CoreUnidadDeTrabajo Unidad,
        Notificador Notificador,
        NotificacionesServicio Servicio) Crear(
            DateTimeOffset? instante = null)
    {
        var reloj = new TimeProviderFijo(instante ?? new DateTimeOffset(2026, 10, 17, 12, 0, 0, TimeSpan.Zero));
        var opciones = new DbContextOptionsBuilder<CoreDbContext>()
            .UseInMemoryDatabase($"notificaciones-{Guid.NewGuid():N}")
            .Options;
        var contexto = new CoreDbContext(opciones, reloj);
        var unidad = new CoreUnidadDeTrabajo(contexto);

        return (
            reloj,
            contexto,
            unidad,
            new Notificador(unidad),
            new NotificacionesServicio(unidad, reloj));
    }

    public static async Task<Usuario> SembrarUsuarioAsync(
        CoreDbContext contexto,
        int id,
        string nombre,
        string correo,
        RolUsuario rol = RolUsuario.Estandar,
        bool activo = true)
    {
        var usuario = new Usuario
        {
            Id = id,
            Nombre = nombre,
            Correo = correo,
            ContraseñaHash = "hash-de-prueba",
            Rol = rol,
            Activo = activo,
        };

        contexto.Usuarios.Add(usuario);
        await contexto.SaveChangesAsync();

        return usuario;
    }
}
