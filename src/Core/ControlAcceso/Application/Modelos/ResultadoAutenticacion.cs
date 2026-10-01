using Core.ControlAcceso.Domain.Entidades;

namespace Core.ControlAcceso.Application.Modelos;

public enum EstadoAutenticacion
{
    Exito,
    CredencialesInvalidas,
    CuentaInactiva,

    /// <summary>La cuenta está temporalmente bloqueada por demasiados intentos fallidos (RF-CA-19).</summary>
    CuentaBloqueada,
}

/// <summary>
/// Resultado de una validación de credenciales. Evita revelar si el correo existe
/// cuando las credenciales son incorrectas (RD-08) y distingue la cuenta inactiva (RF-CA-15)
/// y el bloqueo temporal (RF-CA-19).
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