using Core.Notificaciones.Domain.Enums;

namespace Core.Notificaciones.Application.Interfaces;

/// <summary>
/// Crea avisos internos y su correo asociado dentro de la unidad de trabajo del llamador
/// (RF-NOT-01 a RF-NOT-05, RF-NOT-07, C-04).
/// </summary>
public interface INotificador
{
    /// <summary>
    /// Añade la notificación y, si el destinatario tiene el correo activado, un
    /// <c>CorreoEnCola</c> Pendiente. NO guarda: lo guarda quien llama en la misma transacción.
    /// </summary>
    Task AgregarAsync(
        int usuarioId,
        TipoNotificacion tipo,
        string titulo,
        string mensaje,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Igual que <see cref="AgregarAsync"/> y además confirma la unidad de trabajo.
    /// Lo usan los módulos externos que no comparten transacción con el evento originario.
    /// </summary>
    Task NotificarAsync(
        int usuarioId,
        TipoNotificacion tipo,
        string titulo,
        string mensaje,
        CancellationToken cancellationToken = default);
}
