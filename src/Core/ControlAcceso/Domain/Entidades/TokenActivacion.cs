namespace Core.ControlAcceso.Domain.Entidades;

/// <summary>
/// Token de activación de un solo uso para confirmar el correo de un usuario.
/// <see cref="FechaVencimiento"/> y <see cref="Usado"/> definen si aún puede activar (RF-CA-16).
/// </summary>
public class TokenActivacion
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    /// <summary>Token aleatorio, no adivinable y de un solo uso.</summary>
    public string Codigo { get; set; } = string.Empty;

    public DateTime FechaEmision { get; set; }

    public DateTime FechaVencimiento { get; set; }

    public bool Usado { get; set; }

    public Usuario Usuario { get; set; } = null!;
}