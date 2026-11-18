using Core.ControlAcceso.Application.Interfaces;
using Core.ControlAcceso.Domain.Entidades;
using Core.ControlAcceso.Domain.Enums;
using Core.Notificaciones.Application.Interfaces;
using Core.Notificaciones.Domain.Entidades;
using Core.Notificaciones.Domain.Enums;

namespace Core.Notificaciones.Application.Servicios;

/// <summary>
/// Implementación del notificador del Core. Toda la persistencia pasa por
/// <see cref="ICoreUnidadDeTrabajo"/> (RD-01, RD-02, RD-03, C-04).
/// </summary>
public class Notificador(ICoreUnidadDeTrabajo unidad) : INotificador
{
    public async Task AgregarAsync(
        int usuarioId,
        TipoNotificacion tipo,
        string titulo,
        string mensaje,
        CancellationToken cancellationToken = default)
    {
        var destinatario = await unidad.Usuarios.BuscarPorIdAsync(usuarioId, cancellationToken);
        if (destinatario is null)
        {
            return;
        }

        await unidad.Notificaciones.AgregarAsync(
            new Notificacion
            {
                UsuarioId = usuarioId,
                Tipo = tipo,
                Titulo = titulo,
                Mensaje = mensaje,
            },
            cancellationToken);

        // RF-NOT-07/C-10: sin fila de preferencia el correo está activado; solo se
        // omite cuando existe y está explicitamente desactivado.
        var preferencia = await unidad.PreferenciasNotificacion.ObtenerAsync(usuarioId, cancellationToken);
        if (preferencia?.CorreoActivo == false)
        {
            return;
        }

        await unidad.CorreosEnCola.AgregarAsync(
            new CorreoEnCola
            {
                Destinatario = destinatario.Correo,
                Asunto = titulo,
                Cuerpo = mensaje,
                Estado = EstadoCorreo.Pendiente,
            },
            cancellationToken);
    }

    public async Task NotificarAsync(
        int usuarioId,
        TipoNotificacion tipo,
        string titulo,
        string mensaje,
        CancellationToken cancellationToken = default)
    {
        await AgregarAsync(usuarioId, tipo, titulo, mensaje, cancellationToken);
        await unidad.GuardarCambiosAsync(cancellationToken);
    }
}
