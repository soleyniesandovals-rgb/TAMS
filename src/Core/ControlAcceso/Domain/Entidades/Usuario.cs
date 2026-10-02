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

    /// <summary>
    /// Versión de la sesión vigente. Se incrementa al cerrar sesión o al invalidar
    /// cualquier sesión futura (recuperación de contraseña, etc.): los JWT emitidos
    /// con una versión anterior dejan de ser válidos (RF-CA-12, RF-CA-18).
    /// </summary>
    public int SesionVersion { get; set; }

    /// <summary>Intentos de inicio de sesión con contraseña incorrecta consecutivos (RF-CA-19).</summary>
    public int IntentosFallidosConsecutivos { get; set; }

    /// <summary>
    /// Momento (UTC) en que termina el bloqueo temporal por demasiados intentos
    /// fallidos; null mientras la cuenta no esté bloqueada (RF-CA-19).
    /// </summary>
    public DateTime? BloqueadoHasta { get; set; }

    public DateTime FechaCreacion { get; set; }

    public ICollection<TokenActivacion> TokensActivacion { get; set; } = [];

    public ICollection<CodigoRecuperacion> CodigosRecuperacion { get; set; } = [];
}