using Core.ControlAcceso.Application.Modelos;

namespace Core.ControlAcceso.Application.Interfaces;

/// <summary>
/// Consulta de la cola de correos para administradores (RF-NOT-11).
/// </summary>
public interface IConsultaCorreosEnColaServicio
{
    /// <summary>
    /// Lista los correos encolados, opcionalmente filtrados por estado, del más
    /// reciente al más antiguo. Si el estado es inválido lanza una regla de negocio
    /// con código 400.
    /// </summary>
    Task<IReadOnlyList<CorreoEnColaResumen>> ListarAsync(
        string? estado,
        CancellationToken cancellationToken = default);
}
