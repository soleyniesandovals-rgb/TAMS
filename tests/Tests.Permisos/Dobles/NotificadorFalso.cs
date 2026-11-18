using Core.Notificaciones.Application.Interfaces;
using Core.Notificaciones.Domain.Enums;

namespace Tests.Permisos.Dobles;

/// <summary>Doble en memoria de <see cref="INotificador"/> (RD-12).</summary>
public sealed class NotificadorFalso : INotificador
{
    private readonly List<LlamadaNotificacion> _llamadas = [];

    public IReadOnlyList<LlamadaNotificacion> Llamadas => _llamadas;

    public Task AgregarAsync(
        int usuarioId,
        TipoNotificacion tipo,
        string titulo,
        string mensaje,
        CancellationToken cancellationToken = default)
    {
        _llamadas.Add(new LlamadaNotificacion(usuarioId, tipo, titulo, mensaje));
        return Task.CompletedTask;
    }

    public Task NotificarAsync(
        int usuarioId,
        TipoNotificacion tipo,
        string titulo,
        string mensaje,
        CancellationToken cancellationToken = default)
        => AgregarAsync(usuarioId, tipo, titulo, mensaje, cancellationToken);
}

public sealed record LlamadaNotificacion(
    int UsuarioId,
    TipoNotificacion Tipo,
    string Titulo,
    string Mensaje);
