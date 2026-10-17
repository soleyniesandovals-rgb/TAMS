using Core.ControlAcceso.Domain.Entidades;
using Core.ControlAcceso.Domain.Interfaces;
using Core.Permisos.Domain.Enums;

namespace Core.Permisos.Domain.Entidades;

/// <summary>
/// Solicitud de permiso de un usuario (RF-GP-01). Nace
/// <see cref="EstadoSolicitudPermiso.Pendiente"/> y un administrador la resuelve a
/// aprobada o rechazada (RF-GP-06); las transiciones válidas las controla
/// <see cref="Core.Permisos.Domain.Maquinas.TransicionesPermiso"/> (RD-04).
/// </summary>
public class SolicitudPermiso : ICreacionAuditable
{
    public int Id { get; set; }

    /// <summary>FK al <see cref="Usuario"/> que pide el permiso.</summary>
    public int SolicitanteId { get; set; }

    /// <summary>Tipo de permiso solicitado, por ejemplo "acceso-reportes".</summary>
    public string TipoPermiso { get; set; } = string.Empty;

    public EstadoSolicitudPermiso Estado { get; set; } = EstadoSolicitudPermiso.Pendiente;

    public DateTime FechaCreacion { get; set; }

    /// <summary>FK al <see cref="Usuario"/> que resolvió la solicitud; null mientras esté pendiente.</summary>
    public int? ResueltaPorId { get; set; }

    /// <summary>Momento (UTC) en que se resolvió; null mientras esté pendiente.</summary>
    public DateTime? FechaResolucion { get; set; }

    /// <summary>Motivo del rechazo; obligatorio solo cuando <see cref="Estado"/> es Rechazada.</summary>
    public string? MotivoRechazo { get; set; }

    public Usuario Solicitante { get; set; } = null!;

    public Usuario? ResueltaPor { get; set; }
}
