using Core.ControlAcceso.Domain.Enums;
using Tams.Negocio.Application.Modelos;

namespace Tams.Negocio.Application.Servicios;

/// <summary>
/// Reporte de asistencia por estudiante y materia (RF-REP-02, RF-REP-04).
/// </summary>
public interface IReporteAsistenciaServicio
{
    /// <summary>
    /// Devuelve una fila por asignación y estudiante con los conteos y el
    /// porcentaje de asistencia del rango, según el rol del solicitante.
    /// </summary>
    Task<IReadOnlyList<AsistenciaEstudianteResumen>> ObtenerPorEstudianteYMateriaAsync(
        int usuarioId,
        RolUsuario rol,
        string? desde,
        string? hasta,
        CancellationToken cancellationToken = default);
}
