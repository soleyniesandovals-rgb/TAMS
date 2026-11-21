using Core.ControlAcceso.Domain.Enums;

namespace Core.ControlAcceso.Application.Reglas;

/// <summary>
/// Regla de Control de acceso para interpretar un rol recibido como texto
/// (claim del token o cambio de rol). <c>Enum.TryParse</c> acepta números como
/// "0" y combinaciones como "Administrador,Estandar", que NO son nombres de rol:
/// aquí solo se aceptan los nombres de <see cref="RolUsuario"/> (RD-07, RD-08).
/// </summary>
public static class ReglasRolUsuario
{
    /// <summary>
    /// Devuelve el rol cuando el texto es exactamente uno de sus nombres
    /// (sin distinguir mayúsculas); en cualquier otro caso devuelve null.
    /// </summary>
    public static RolUsuario? Obtener(string? nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            return null;
        }

        var texto = nombre.Trim();

        return Enum.TryParse<RolUsuario>(texto, ignoreCase: true, out var rol)
               && Enum.IsDefined(rol)
               && string.Equals(rol.ToString(), texto, StringComparison.OrdinalIgnoreCase)
            ? rol
            : null;
    }
}
