using Core.Notificaciones.Domain.Entidades;

namespace Core.Notificaciones.Application.Interfaces;

/// <summary>
/// Consulta y mantenimiento de las notificaciones y preferencias del usuario autenticado
/// (RF-NOT-04 a RF-NOT-07).
/// </summary>
public interface INotificacionesServicio
{
    /// <summary>Devuelve las notificaciones propias, de la más reciente a la más antigua (RF-NOT-04).</summary>
    Task<IReadOnlyList<Notificacion>> ListarPropiasAsync(
        int usuarioId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marca como leída una notificación propia. Si no existe o no pertenece al usuario,
    /// lanza <c>ReglaNegocioExcepcion</c> con código 404 (RF-NOT-05, C-07, C-08).
    /// </summary>
    Task<Notificacion> MarcarLeidaAsync(
        int usuarioId,
        int id,
        CancellationToken cancellationToken = default);

    /// <summary>Devuelve true si no hay fila de preferencia o si el correo está activado (RF-NOT-07).</summary>
    Task<bool> ObtenerPreferenciaAsync(
        int usuarioId,
        CancellationToken cancellationToken = default);

    /// <summary>Crea o actualiza la preferencia de correo del usuario (RF-NOT-07).</summary>
    Task CambiarPreferenciaAsync(
        int usuarioId,
        bool correoActivo,
        CancellationToken cancellationToken = default);
}
