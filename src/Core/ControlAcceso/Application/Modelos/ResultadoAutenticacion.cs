using Core.ControlAcceso.Domain.Entidades;

namespace Core.ControlAcceso.Application.Modelos;

public enum EstadoAutenticacion
{
    Exito,
    CredencialesInvalidas,
    CuentaInactiva,
}

/// <summary>
/// Resultado de una validación de credenciales. Evita revelar si el correo existe
/// cuando las credenciales son incorrectas (RD-08) y distingue la cuenta inactiva (RF-CA-15).
/// </summary>
public sealed record ResultadoAutenticacion(EstadoAutenticacion Estado, Usuario? Usuario);