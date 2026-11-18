using Core.ControlAcceso.Application.Interfaces;
using Core.ControlAcceso.Domain.Excepciones;
using Core.Notificaciones.Application.Interfaces;
using Core.Notificaciones.Domain.Entidades;

namespace Core.Notificaciones.Application.Servicios;

/// <summary>
/// Operaciones del usuario sobre sus propias notificaciones (RF-NOT-04 a RF-NOT-07).
/// RD-11/RD-12: la fecha de lectura sale del <see cref="TimeProvider"/> inyectado.
/// </summary>
public class NotificacionesServicio(
    ICoreUnidadDeTrabajo unidad,
    TimeProvider timeProvider) : INotificacionesServicio
{
    public Task<IReadOnlyList<Notificacion>> ListarPropiasAsync(
        int usuarioId,
        CancellationToken cancellationToken = default)
        => unidad.Notificaciones.ListarPorUsuarioAsync(usuarioId, cancellationToken);

    public async Task<Notificacion> MarcarLeidaAsync(
        int usuarioId,
        int id,
        CancellationToken cancellationToken = default)
    {
        var notificacion = await unidad.Notificaciones.BuscarPorIdAsync(id, cancellationToken);

        // C-08: no se distingue entre inexistente y ajena; en ningún caso se modifica nada.
        if (notificacion is null || notificacion.UsuarioId != usuarioId)
        {
            throw new ReglaNegocioExcepcion("La notificación no existe.", 404);
        }

        // C-07: marcar dos veces no reescribe la fecha de la primera lectura.
        if (!notificacion.Leida)
        {
            notificacion.Leida = true;
            notificacion.FechaLectura = timeProvider.GetUtcNow().UtcDateTime;
            await unidad.GuardarCambiosAsync(cancellationToken);
        }

        return notificacion;
    }

    public async Task<bool> ObtenerPreferenciaAsync(
        int usuarioId,
        CancellationToken cancellationToken = default)
    {
        var preferencia = await unidad.PreferenciasNotificacion.ObtenerAsync(usuarioId, cancellationToken);
        return preferencia?.CorreoActivo ?? true;
    }

    public async Task CambiarPreferenciaAsync(
        int usuarioId,
        bool correoActivo,
        CancellationToken cancellationToken = default)
    {
        var preferencia = await unidad.PreferenciasNotificacion.ObtenerAsync(usuarioId, cancellationToken);

        if (preferencia is null)
        {
            await unidad.PreferenciasNotificacion.AgregarAsync(
                new PreferenciaNotificacion
                {
                    UsuarioId = usuarioId,
                    CorreoActivo = correoActivo,
                },
                cancellationToken);
        }
        else
        {
            preferencia.CorreoActivo = correoActivo;
        }

        await unidad.GuardarCambiosAsync(cancellationToken);
    }
}
