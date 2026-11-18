using Core.ControlAcceso.Domain.Enums;
using Core.Reportes.Application.Modelos;

namespace Core.Reportes.Application.Interfaces;

/// <summary>
/// Reporte de solicitudes de permiso agrupadas por mes y estado (RF-REP-01, RF-REP-04).
/// </summary>
public interface IReporteSolicitudesPermisoServicio
{
    /// <summary>
    /// Devuelve las cantidades por mes (UTC) y estado dentro del rango,
    /// visibles para el usuario según su rol.
    /// </summary>
    Task<IReadOnlyList<SolicitudPermisoAgrupadaResumen>> ObtenerPorMesYEstadoAsync(
        int usuarioId,
        RolUsuario rol,
        string? desde,
        string? hasta,
        CancellationToken cancellationToken = default);
}
