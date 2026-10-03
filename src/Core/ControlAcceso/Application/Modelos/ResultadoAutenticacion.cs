using Core.ControlAcceso.Domain.Entidades;

namespace Core.ControlAcceso.Application.Modelos;

public enum EstadoAutenticacion
{
    Exito,
    CredencialesInvalidas,
    CuentaInactiva,

    /// <summary>La cuenta está temporalmente bloqueada por demasiados intentos fallidos (RF-CA-19).</summary>
    CuentaBloqueada,

    /// <summary>
    /// La cuenta tiene un restablecimiento forzado por un Administrador y aún no se
    /// definió una contraseña nueva (RF-CA-13): el login se rechaza aunque la
    /// contraseña sea la correcta.
    /// </summary>
    RestablecimientoPendiente,
}

/// <summary>
/// Resultado de una validación de credenciales. Evita revelar si el correo existe
/// cuando las credenciales son incorrectas (RD-08) y distingue la cuenta inactiva (RF-CA-15),
/// el bloqueo temporal (RF-CA-19) y el restablecimiento pendiente (RF-CA-13).
/// </summary>
public sealed record ResultadoAutenticacion(EstadoAutenticacion Estado, Usuario? Usuario)
{
    /// <summary>Fin del bloqueo temporal; se informa solo en <see cref="EstadoAutenticacion.CuentaBloqueada"/> (RF-CA-19).</summary>
    public DateTime? BloqueadoHasta { get; init; }

    /// <summary>JWT emitido en <see cref="EstadoAutenticacion.Exito"/> (RF-CA-03).</summary>
    public string? Token { get; init; }

    /// <summary>Momento (UTC) en que expira el <see cref="Token"/> (RF-CA-03).</summary>
    public DateTime? ExpiraEn { get; init; }
}