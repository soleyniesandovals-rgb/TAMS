using Core.ControlAcceso.Domain.Entidades;
using Core.ControlAcceso.Domain.Enums;

namespace Core.ControlAcceso.Application.Interfaces;

public interface ICorreosEnColaRepositorio
{
    Task AgregarAsync(CorreoEnCola correo, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CorreoEnCola>> ObtenerPendientesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Reclama un correo Pendiente marcándolo Enviando mediante un UPDATE condicionado
    /// al estado actual: devuelve false si otra ejecución ya lo tomó (RF-NOT-09).
    /// </summary>
    Task<bool> ReclamarAsync(int id, CancellationToken cancellationToken = default);

    Task MarcarEnviadoAsync(
        int id,
        DateTime fechaEnvioUtc,
        int intentos,
        DateTime fechaUltimoIntentoUtc,
        CancellationToken cancellationToken = default);

    Task RegistrarFalloAsync(
        int id,
        int intentos,
        DateTime fechaUltimoIntentoUtc,
        string ultimoError,
        bool quedaFallido,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CorreoEnCola>> ListarAsync(
        EstadoCorreo? estado,
        CancellationToken cancellationToken = default);
}