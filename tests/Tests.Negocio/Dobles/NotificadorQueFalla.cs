using Core.Notificaciones.Application.Interfaces;
using Core.Notificaciones.Domain.Enums;

namespace Tests.Negocio.Dobles;

/// <summary>
/// Doble que simula un fallo al notificar (C-06). Sirve para comprobar que la
/// asignación ya confirmada no se revierte ni se propaga el error.
/// </summary>
public sealed class NotificadorQueFalla : INotificador
{
    public Task AgregarAsync(
        int usuarioId,
        TipoNotificacion tipo,
        string titulo,
        string mensaje,
        CancellationToken cancellationToken = default)
        => throw new InvalidOperationException("Fallo simulado al notificar.");

    public Task NotificarAsync(
        int usuarioId,
        TipoNotificacion tipo,
        string titulo,
        string mensaje,
        CancellationToken cancellationToken = default)
        => throw new InvalidOperationException("Fallo simulado al notificar.");
}
