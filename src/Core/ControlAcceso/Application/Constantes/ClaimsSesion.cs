namespace Core.ControlAcceso.Application.Constantes;

/// <summary>Tipos de claim usados por los JWT de sesión del Core.</summary>
public static class ClaimsSesion
{
    /// <summary>
    /// Claim personalizado que transporta la versión de sesión del usuario
    /// (RF-CA-12, RF-CA-18): el validador Bearer la compara contra la base de datos
    /// en cada petición para invalidar JWTs viejos sin lista de revocación.
    /// </summary>
    public const string SesionVersion = "sesion_version";
}