using Core.ControlAcceso.Domain.Entidades;

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

    Task MarcarEnviadoAsync(int id, DateTime fechaEnvioUtc, CancellationToken cancellationToken = default);

    Task ReactivarAsync(int id, CancellationToken cancellationToken = default);
}