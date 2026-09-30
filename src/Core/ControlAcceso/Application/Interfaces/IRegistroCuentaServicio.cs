using Core.ControlAcceso.Domain.Entidades;

namespace Core.ControlAcceso.Application.Interfaces;

public interface IRegistroCuentaServicio
{
    /// <summary>
    /// Registra una cuenta nueva: valida la política de contraseña (RF-CA-14),
    /// rechaza correos duplicados (RF-CA-01), hashea la contraseña (RF-CA-02),
    /// crea la cuenta inactiva con un token y encola el correo (RF-CA-15).
    /// </summary>
    Task<Usuario> RegistrarseAsync(string nombre, string correo, string contrasena, CancellationToken cancellationToken = default);
}