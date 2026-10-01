namespace Core.ControlAcceso.Application.Interfaces;

public interface ISesionServicio
{
    /// <summary>
    /// Valida que la versión de sesión del JWT coincida con la vigente en base de
    /// datos; si no, el token pertenece a una sesión ya cerrada (RF-CA-12, RF-CA-18).
    /// </summary>
    Task<bool> ValidarSesionAsync(int usuarioId, int sesionVersion, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalida la sesión actual (y todas las anteriores) del usuario incrementando
    /// su SesionVersion (RF-CA-18).
    /// </summary>
    Task CerrarSesionAsync(int usuarioId, CancellationToken cancellationToken = default);
}