using Core.ControlAcceso.Domain.Enums;
using Core.ControlAcceso.Domain.Interfaces;

namespace Core.ControlAcceso.Domain.Entidades;

/// <summary>
/// Correo encolado para envío asíncrono (versión mínima de esta fase).
/// El envío real lo hará el procesador de la cola en fases posteriores.
/// </summary>
public class CorreoEnCola : ICreacionAuditable
{
    public int Id { get; set; }

    public string Destinatario { get; set; } = string.Empty;

    public string Asunto { get; set; } = string.Empty;

    public string Cuerpo { get; set; } = string.Empty;

    public EstadoCorreo Estado { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaEnvio { get; set; }
}