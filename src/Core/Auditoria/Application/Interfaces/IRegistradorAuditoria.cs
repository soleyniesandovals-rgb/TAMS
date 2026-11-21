using Core.Auditoria.Domain.Enums;

namespace Core.Auditoria.Application.Interfaces;

/// <summary>
/// Registra un evento auditable en el historial (RF-AUD-01, CA-01). Solo agrega el
/// registro a la unidad de trabajo; quien guarda es la operación auditada (CA-12).
/// </summary>
public interface IRegistradorAuditoria
{
    Task RegistrarAsync(
        int usuarioId,
        AccionAuditoria accion,
        EntidadAuditoria entidad,
        int entidadId,
        string? valorAnterior,
        string? valorNuevo,
        CancellationToken cancellationToken = default);
}
