using Core.ControlAcceso.Domain.Enums;
using Core.ControlAcceso.Domain.Interfaces;

namespace Core.ControlAcceso.Domain.Entidades;

/// <summary>
/// Usuario del sistema de control de acceso.
/// Nace con <see cref="Activo"/> en false y se activa con un token (RF-CA-15, RF-CA-16).
/// </summary>
public class Usuario : ICreacionAuditable
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Correo { get; set; } = string.Empty;

    public string ContraseñaHash { get; set; } = string.Empty;

    public RolUsuario Rol { get; set; }

    public bool Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public ICollection<TokenActivacion> TokensActivacion { get; set; } = [];
}