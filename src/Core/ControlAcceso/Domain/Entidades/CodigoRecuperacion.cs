namespace Core.ControlAcceso.Domain.Entidades;

/// <summary>
/// Código de recuperación de contraseña de un solo uso (RF-CA-09, RF-CA-10, RF-CA-11).
/// <see cref="FechaVencimiento"/> y <see cref="Usado"/> definen si aún permite
/// establecer una contraseña nueva. El código es aleatorio y no adivinable, con el
/// mismo patrón que <see cref="TokenActivacion"/>.
/// </summary>
public class CodigoRecuperacion
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    /// <summary>Código aleatorio, no adivinable y de un solo uso.</summary>
    public string Codigo { get; set; } = string.Empty;

    public DateTime FechaEmision { get; set; }

    public DateTime FechaVencimiento { get; set; }

    public bool Usado { get; set; }

    public Usuario Usuario { get; set; } = null!;
}
