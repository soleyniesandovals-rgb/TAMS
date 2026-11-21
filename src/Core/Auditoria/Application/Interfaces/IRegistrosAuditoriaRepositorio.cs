using Core.Auditoria.Domain.Entidades;

namespace Core.Auditoria.Application.Interfaces;

/// <summary>
/// Repositorio de solo agrego del historial (RF-AUD-01, RF-AUD-06, CA-14): no expone
/// operaciones de editar ni borrar. Las consultas se agregan en un bloque posterior.
/// </summary>
public interface IRegistrosAuditoriaRepositorio
{
    Task AgregarAsync(RegistroAuditoria registro, CancellationToken cancellationToken = default);
}
