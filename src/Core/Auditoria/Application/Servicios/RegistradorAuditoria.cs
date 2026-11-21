using Core.Auditoria.Application.Interfaces;
using Core.Auditoria.Domain.Entidades;
using Core.Auditoria.Domain.Enums;
using Core.ControlAcceso.Application.Interfaces;

namespace Core.Auditoria.Application.Servicios;

/// <summary>
/// Sella la fecha con el <see cref="TimeProvider"/> inyectado (RD-11) y agrega el
/// registro a la misma unidad de trabajo que la operación auditada, SIN guardar
/// (CA-12). Todavía no se llama desde ningún evento (RF-AUD-01).
/// </summary>
public class RegistradorAuditoria(
    ICoreUnidadDeTrabajo unidad,
    TimeProvider timeProvider) : IRegistradorAuditoria
{
    public async Task RegistrarAsync(
        int usuarioId,
        AccionAuditoria accion,
        EntidadAuditoria entidad,
        int entidadId,
        string? valorAnterior,
        string? valorNuevo,
        CancellationToken cancellationToken = default)
    {
        var registro = RegistroAuditoria.Crear(
            usuarioId,
            accion,
            entidad,
            entidadId,
            timeProvider.GetUtcNow().UtcDateTime,
            valorAnterior,
            valorNuevo);

        await unidad.RegistrosAuditoria.AgregarAsync(registro, cancellationToken);
    }
}
