using Core.Notificaciones.Domain.Entidades;

namespace Core.Notificaciones.Application.Interfaces;

/// <summary>Acceso a la preferencia de correo de las notificaciones (RF-NOT-07).</summary>
public interface IPreferenciasNotificacionRepositorio
{
    Task<PreferenciaNotificacion?> ObtenerAsync(int usuarioId, CancellationToken cancellationToken = default);

    Task AgregarAsync(PreferenciaNotificacion preferencia, CancellationToken cancellationToken = default);
}
