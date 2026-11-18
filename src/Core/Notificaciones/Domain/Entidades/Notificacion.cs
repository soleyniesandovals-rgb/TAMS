using Core.ControlAcceso.Domain.Interfaces;
using Core.Notificaciones.Domain.Enums;

namespace Core.Notificaciones.Domain.Entidades;

/// <summary>
/// Aviso interno de un evento del sistema para un usuario (RF-NOT-04).
/// </summary>
public class Notificacion : ICreacionAuditable
{
    public int Id { get; set; }

    /// <summary>Id del usuario destinatario en el Core.</summary>
    public int UsuarioId { get; set; }

    public TipoNotificacion Tipo { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string Mensaje { get; set; } = string.Empty;

    public bool Leida { get; set; } = false;

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaLectura { get; set; }
}
