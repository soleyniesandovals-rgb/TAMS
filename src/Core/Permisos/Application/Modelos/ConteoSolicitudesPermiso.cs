using Core.Permisos.Domain.Enums;

namespace Core.Permisos.Application.Modelos;

/// <summary>
/// Fila ya agregada por la base de datos con el total de solicitudes de un mes y
/// un estado (RF-REP-01, C-20). La agrupación se hace en el repositorio de la
/// pieza dueña de los datos (RD-01), sobre <c>IQueryable</c>, para que SQL Server
/// ejecute un <c>GROUP BY</c> y no se traiga el listado completo a memoria.
/// </summary>
/// <param name="Anio">Año de <c>FechaCreacion</c> en UTC.</param>
/// <param name="Mes">Mes de <c>FechaCreacion</c> en UTC (1 a 12).</param>
/// <param name="Estado">Estado de la solicitud.</param>
/// <param name="Cantidad">Cantidad de solicitudes del mes y estado.</param>
public sealed record ConteoSolicitudesPermiso(
    int Anio,
    int Mes,
    EstadoSolicitudPermiso Estado,
    int Cantidad);
