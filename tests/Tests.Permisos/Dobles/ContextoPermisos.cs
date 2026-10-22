using Core.Permisos.Application.Interfaces;
using Core.Permisos.Application.Servicios;
using Core.Permisos.Domain.Entidades;
using Core.Permisos.Domain.Enums;

namespace Tests.Permisos.Dobles;

/// <summary>
/// Monta el servicio de permisos con los dobles en memoria (RD-12). Cada llamada devuelve un
/// contexto nuevo, de modo que ninguna prueba comparte estado ni datos sembrados con otra.
/// </summary>
public static class ContextoPermisos
{
    /// <summary>Instante fijo por defecto: 17/10/2026 12:00:00 UTC.</summary>
    public static readonly DateTime InstantePorDefectoUtc = new(2026, 10, 17, 12, 0, 0, DateTimeKind.Utc);

    /// <summary>Contexto con el reloj fijo por defecto.</summary>
    public static (GestionPermisosServicio Servicio, CoreUnidadDeTrabajoFalsa Unidad, SolicitudesPermisoRepositorioFalso Solicitudes, PermisosActivosRepositorioFalso PermisosActivos) Crear()
        => Crear(TimeProviderFijo.CrearUtc(2026, 10, 17, 12, 0, 0));

    /// <summary>Contexto con el reloj que indica la prueba.</summary>
    public static (GestionPermisosServicio Servicio, CoreUnidadDeTrabajoFalsa Unidad, SolicitudesPermisoRepositorioFalso Solicitudes, PermisosActivosRepositorioFalso PermisosActivos) Crear(TimeProvider timeProvider)
    {
        var solicitudes = new SolicitudesPermisoRepositorioFalso();
        var permisosActivos = new PermisosActivosRepositorioFalso();
        var unidad = new CoreUnidadDeTrabajoFalsa(solicitudes, permisosActivos);
        var servicio = new GestionPermisosServicio(unidad, timeProvider);

        return (servicio, unidad, solicitudes, permisosActivos);
    }

    /// <summary>
    /// Siembra una solicitud en un estado concreto sin pasar por el servicio. Sirve para probar
    /// las transiciones desde estados que el servicio no produce por sí solo (Aprobada,
    /// Aplicada, Rechazada), sin depender del orden ni del resultado de otras pruebas.
    /// </summary>
    public static async Task<SolicitudPermiso> SembrarAsync(
        ISolicitudesPermisoRepositorio solicitudes,
        int solicitanteId,
        string tipoPermiso,
        EstadoSolicitudPermiso estado)
    {
        var solicitud = new SolicitudPermiso
        {
            SolicitanteId = solicitanteId,
            TipoPermiso = tipoPermiso,
            Estado = estado,
        };

        await solicitudes.AgregarAsync(solicitud, CancellationToken.None);

        return solicitud;
    }
}
