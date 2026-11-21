namespace Core.Auditoria.Domain.Enums;

/// <summary>
/// Entidades auditables del historial (CA-01, RF-AUD-01). Los nombres son exactamente
/// los publicados en docs/criterios-auditoria.md.
/// </summary>
public enum EntidadAuditoria
{
    SolicitudPermiso,
    Documento,
    Usuario,
}
