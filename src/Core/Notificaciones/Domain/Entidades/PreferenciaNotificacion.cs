namespace Core.Notificaciones.Domain.Entidades;

/// <summary>
/// Preferencia de envío de correo para las notificaciones de un usuario (RF-NOT-07).
/// La ausencia de fila significa «correo activado».
/// </summary>
public class PreferenciaNotificacion
{
    public int UsuarioId { get; set; }

    public bool CorreoActivo { get; set; } = true;
}
