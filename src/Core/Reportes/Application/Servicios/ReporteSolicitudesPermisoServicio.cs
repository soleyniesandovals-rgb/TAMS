using Core.ControlAcceso.Application.Interfaces;
using Core.ControlAcceso.Domain.Enums;
using Core.Reportes.Application.Interfaces;
using Core.Reportes.Application.Modelos;

namespace Core.Reportes.Application.Servicios;

/// <summary>
/// Reporte del Core: «¿cuántas solicitudes de permiso hubo por mes y estado?»
/// (RF-REP-01, RF-REP-03, RF-REP-04, RF-REP-05, RD-02). La agrupación la hace
/// la base de datos a través del repositorio de SolicitudPermiso (RD-01); aquí
/// solo se valida el rango, se decide el filtro por rol y se formatea la salida.
/// </summary>
public class ReporteSolicitudesPermisoServicio(ICoreUnidadDeTrabajo unidad)
    : IReporteSolicitudesPermisoServicio
{
    public async Task<IReadOnlyList<SolicitudPermisoAgrupadaResumen>> ObtenerPorMesYEstadoAsync(
        int usuarioId,
        RolUsuario rol,
        string? desde,
        string? hasta,
        CancellationToken cancellationToken = default)
    {
        // RF-REP-03/RD-07: una sola validación de rango para los dos reportes.
        var rango = RangoFechasReporte.Crear(desde, hasta);

        // RF-CA-05: el controlador solo exige sesión; la decisión de qué datos ve
        // cada rol la toma el servicio. Administrador = todas; Estándar = solo
        // las suyas (SolicitanteId = su id).
        var solicitanteId = rol == RolUsuario.Administrador ? (int?)null : usuarioId;

        var conteos = await unidad.SolicitudesPermiso.ContarPorMesYEstadoAsync(
            rango.DesdeUtc,
            rango.HastaExclusivoUtc,
            solicitanteId,
            cancellationToken);

        return conteos
            .Select(c => new SolicitudPermisoAgrupadaResumen(
                FormattableString.Invariant($"{c.Anio:D4}-{c.Mes:D2}"),
                c.Estado.ToString(),
                c.Cantidad))
            .OrderBy(fila => fila.Mes, StringComparer.Ordinal)
            .ThenBy(fila => fila.Estado, StringComparer.Ordinal)
            .ToList();
    }
}
