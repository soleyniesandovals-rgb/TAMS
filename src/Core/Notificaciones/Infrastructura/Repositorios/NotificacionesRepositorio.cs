using Core.ControlAcceso.Infrastructura.Persistence;
using Core.Notificaciones.Application.Interfaces;
using Core.Notificaciones.Domain.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Core.Notificaciones.Infrastructura.Repositorios;

public class NotificacionesRepositorio(CoreDbContext context) : INotificacionesRepositorio
{
    public async Task AgregarAsync(Notificacion notificacion, CancellationToken cancellationToken = default)
        => await context.Notificaciones.AddAsync(notificacion, cancellationToken);

    public async Task<Notificacion?> BuscarPorIdAsync(int id, CancellationToken cancellationToken = default)
        => await context.Notificaciones.FirstOrDefaultAsync(n => n.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Notificacion>> ListarPorUsuarioAsync(
        int usuarioId,
        CancellationToken cancellationToken = default)
        => await context.Notificaciones
            .AsNoTracking()
            .Where(n => n.UsuarioId == usuarioId)
            .OrderByDescending(n => n.FechaCreacion)
            .ThenByDescending(n => n.Id)
            .ToListAsync(cancellationToken);
}
