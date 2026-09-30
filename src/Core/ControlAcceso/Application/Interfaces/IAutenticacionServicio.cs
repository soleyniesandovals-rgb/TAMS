using Core.ControlAcceso.Application.Modelos;

namespace Core.ControlAcceso.Application.Interfaces;

public interface IAutenticacionServicio
{
    /// <summary>
    /// Valida un correo y una contraseña. Una cuenta inactiva se rechaza indicando
    /// que no está activa (RF-CA-15). Las credenciales incorrectas no revelan cuál
    /// de los dos datos falló ni si el correo existe (RD-08).
    /// </summary>
    Task<ResultadoAutenticacion> AutenticarAsync(string correo, string contrasena, CancellationToken cancellationToken = default);
}