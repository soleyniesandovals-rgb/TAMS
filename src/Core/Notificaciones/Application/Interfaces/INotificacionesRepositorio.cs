using Core.Notificaciones.Domain.Entidades;

namespace Core.Notificaciones.Application.Interfaces;

/// <summary>Acceso a las notificaciones persistidas (RF-NOT-04 a RF-NOT-06).</summary>
public interface INotificacionesRepositorio
{
    Task AgregarAsync(Notificacion notificacion, CancellationToken cancellationToken = default);

    Task<Notificacion?> BuscarPorIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Notificacion>> ListarPorUsuarioAsync(int usuarioId, CancellationToken cancellationToken = default);
}
