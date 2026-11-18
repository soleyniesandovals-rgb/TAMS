using Core.ControlAcceso.Infrastructura.Persistence;
using Core.Notificaciones.Application.Interfaces;
using Core.Notificaciones.Domain.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Core.Notificaciones.Infrastructura.Repositorios;

public class PreferenciasNotificacionRepositorio(CoreDbContext context) : IPreferenciasNotificacionRepositorio
{
    public async Task<PreferenciaNotificacion?> ObtenerAsync(int usuarioId, CancellationToken cancellationToken = default)
        => await context.PreferenciasNotificacion.FirstOrDefaultAsync(p => p.UsuarioId == usuarioId, cancellationToken);

    public async Task AgregarAsync(PreferenciaNotificacion preferencia, CancellationToken cancellationToken = default)
        => await context.PreferenciasNotificacion.AddAsync(preferencia, cancellationToken);
}
