using Core.Auditoria.Domain.Enums;

namespace Core.Auditoria.Application.Modelos;

/// <summary>
/// Filtros ya validados de la consulta del historial (RF-AUD-07, CA-15 a CA-18).
/// Los extremos de fecha son UTC e INCLUSIVOS: <see cref="HastaUtc"/> es el último
/// instante del día indicado, así que cuenta cualquier hora de ese día (CA-16).
/// </summary>
public sealed record FiltrosConsultaAuditoria(
    int? UsuarioId,
    EntidadAuditoria? Entidad,
    DateTime? DesdeUtc,
    DateTime? HastaUtc)
{
    /// <summary>Tope establecido para el historial (CA-15).</summary>
    public const int MaximoRegistros = 500;
}
