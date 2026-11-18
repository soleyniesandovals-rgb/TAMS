namespace Core.Reportes.Application.Modelos;

/// <summary>
/// Fila del reporte de solicitudes de permiso (RF-REP-01, C-20): una combinación
/// de mes y estado con su cantidad, no una solicitud individual.
/// </summary>
/// <param name="Mes">Mes en formato <c>AAAA-MM</c> según <c>FechaCreacion</c> en UTC.</param>
/// <param name="Estado">Estado de las solicitudes del grupo.</param>
/// <param name="Cantidad">Cantidad de solicitudes del mes y estado.</param>
public sealed record SolicitudPermisoAgrupadaResumen(
    string Mes,
    string Estado,
    int Cantidad);
