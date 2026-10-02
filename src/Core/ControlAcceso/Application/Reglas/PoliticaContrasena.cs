using Core.ControlAcceso.Domain.Excepciones;

namespace Core.ControlAcceso.Application.Reglas;

/// <summary>
/// Política de contraseñas del sistema (RF-CA-14): mínimo 8 caracteres e incluir
/// letras y números. Centralizada para que el registro, la recuperación y el cambio
/// de contraseña apliquen exactamente la misma regla (RD-02).
/// </summary>
public static class PoliticaContrasena
{
    public const int LongitudMinima = 8;

    /// <summary>
    /// Valida la contraseña; lanza <see cref="ReglaNegocioExcepcion"/> con un mensaje
    /// controlado (RD-07, RD-08) si no cumple la política.
    /// </summary>
    public static void Exigir(string contrasena)
    {
        if (string.IsNullOrEmpty(contrasena)
            || contrasena.Length < LongitudMinima
            || !contrasena.Any(char.IsLetter)
            || !contrasena.Any(char.IsDigit))
        {
            throw new ReglaNegocioExcepcion(
                "La contraseña debe tener al menos 8 caracteres e incluir letras y números.");
        }
    }
}
