using Core.Auditoria.Domain.Enums;

namespace Core.Auditoria.Application.Modelos;

/// <summary>
/// Registro del historial devuelto al Administrador (RF-AUD-07, CA-15).
/// <see cref="FechaHora"/> viaja en UTC con marca <see cref="DateTimeKind.Utc"/>
/// para que se serialice con <c>Z</c> (RD-11).
/// </summary>
public sealed record RegistroAuditoriaResumen(
    int Id,
    int UsuarioId,
    AccionAuditoria Accion,
    EntidadAuditoria Entidad,
    int EntidadId,
    DateTime FechaHora,
    string? ValorAnterior,
    string? ValorNuevo);
