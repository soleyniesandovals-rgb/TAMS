using Core.ControlAcceso.Domain.Entidades;
using Core.ControlAcceso.Domain.Interfaces;

namespace Core.Permisos.Domain.Entidades;

/// <summary>
/// Permiso otorgado a un usuario (RF-GP-05). Se crea al aplicar una
/// <see cref="SolicitudPermiso"/> aprobada y conserva el id de esa solicitud para la
/// trazabilidad. Un usuario no puede tener el mismo <see cref="TipoPermiso"/> dos veces.
/// </summary>
public class PermisoActivo : ICreacionAuditable
{
    public int Id { get; set; }

    /// <summary>FK al <see cref="Usuario"/> que tiene el permiso otorgado.</summary>
    public int UsuarioId { get; set; }

    public string TipoPermiso { get; set; } = string.Empty;

    /// <summary>FK a la <see cref="SolicitudPermiso"/> que originó el permiso (trazabilidad).</summary>
    public int SolicitudPermisoId { get; set; }

    public DateTime FechaCreacion { get; set; }

    public Usuario Usuario { get; set; } = null!;

    public SolicitudPermiso SolicitudPermiso { get; set; } = null!;
}
