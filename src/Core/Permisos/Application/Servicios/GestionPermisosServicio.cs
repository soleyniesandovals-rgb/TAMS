using Core.ControlAcceso.Application.Interfaces;
using Core.ControlAcceso.Domain.Excepciones;
using Core.Permisos.Application.Interfaces;
using Core.Permisos.Application.Modelos;
using Core.Permisos.Domain.Entidades;
using Core.Permisos.Domain.Enums;
using Core.Permisos.Domain.Maquinas;

namespace Core.Permisos.Application.Servicios;

/// <summary>
/// Servicio de gestión de permisos (RF-GP-01 a RF-GP-07).
/// RD-02: toda la lógica de negocio vive aquí; el controlador solo traduce a HTTP.
/// RD-03: usa la <see cref="ICoreUnidadDeTrabajo"/> del Core; no conoce el módulo de negocio.
/// RD-11/RD-12: las fechas se toman del <see cref="TimeProvider"/> inyectado, nunca de
/// <c>DateTime.Now</c>/<c>DateTime.UtcNow</c>.
/// </summary>
public class GestionPermisosServicio(ICoreUnidadDeTrabajo unidad, TimeProvider timeProvider) : IGestionPermisosServicio
{
    /// <summary>Longitud máxima de un tipo de permiso; coincide con la columna (100).</summary>
    private const int LongitudMaximaTipoPermiso = 100;

    public async Task<SolicitudPermisoResumen> CrearAsync(
        int solicitanteId,
        string tipoPermiso,
        CancellationToken cancellationToken = default)
    {
        // RD-07: toda entrada externa se valida antes de usarse (mensaje controlado, RD-08).
        if (string.IsNullOrWhiteSpace(tipoPermiso))
        {
            throw new ReglaNegocioExcepcion("El tipo de permiso es obligatorio.");
        }

        var tipoNormalizado = tipoPermiso.Trim();

        if (tipoNormalizado.Length > LongitudMaximaTipoPermiso)
        {
            throw new ReglaNegocioExcepcion($"El tipo de permiso no puede superar los {LongitudMaximaTipoPermiso} caracteres.");
        }

        // RF-GP-01: la solicitud nace Pendiente y espera resolución manual.
        var solicitud = new SolicitudPermiso
        {
            SolicitanteId = solicitanteId,
            TipoPermiso = tipoNormalizado,
            Estado = EstadoSolicitudPermiso.Pendiente,
        };

        await unidad.SolicitudesPermiso.AgregarAsync(solicitud, cancellationToken);
        await unidad.GuardarCambiosAsync(cancellationToken);

        return AResumen(solicitud);
    }

    public async Task<IReadOnlyList<SolicitudPermisoResumen>> ListarPropiasAsync(
        int usuarioId,
        CancellationToken cancellationToken = default)
    {
        // RF-GP-02: el usuario solo ve sus propias solicitudes.
        var solicitudes = await unidad.SolicitudesPermiso.ListarPorSolicitanteAsync(usuarioId, cancellationToken);
        return solicitudes.Select(AResumen).ToList();
    }

    public async Task<IReadOnlyList<SolicitudPermisoResumen>> ListarPendientesAsync(
        CancellationToken cancellationToken = default)
    {
        // RF-GP-03: bandeja de pendientes para el Administrador (el controlador ya exige el rol).
        var solicitudes = await unidad.SolicitudesPermiso.ListarPendientesAsync(cancellationToken);
        return solicitudes.Select(AResumen).ToList();
    }

    public async Task<SolicitudPermisoResumen> AprobarAsync(
        int solicitudId,
        int administradorId,
        CancellationToken cancellationToken = default)
    {
        var solicitud = await ObtenerRequeridaAsync(solicitudId, cancellationToken);

        // RF-GP-07: nadie resuelve su propia solicitud. Se comprueba ANTES de validar la
        // transición para que el motivo del rechazo sea inequívoco (no un "transición no
        // permitida" engañoso).
        ExigirNoAutoResolucion(solicitud, administradorId);

        // RD-04/RF-GP-06: la máquina de estados es la única fuente de verdad. Si no está
        // habilitada (p. ej. ya resuelta), TransicionNoPermitidaExcepcion se propaga tal cual.
        TransicionesPermiso.Exigir(solicitud.Estado, EstadoSolicitudPermiso.Aprobada);

        solicitud.Estado = EstadoSolicitudPermiso.Aprobada;
        solicitud.ResueltaPorId = administradorId;
        solicitud.FechaResolucion = AhoraUtc();

        // RF-GP-05: la aprobación aplica el permiso en la misma operación, sin paso manual.
        TransicionesPermiso.Exigir(solicitud.Estado, EstadoSolicitudPermiso.Aplicada);
        solicitud.Estado = EstadoSolicitudPermiso.Aplicada;

        if (!await unidad.PermisosActivos.ExistePermisoAsync(solicitud.SolicitanteId, solicitud.TipoPermiso, cancellationToken))
        {
            await unidad.PermisosActivos.AgregarAsync(
                new PermisoActivo
                {
                    UsuarioId = solicitud.SolicitanteId,
                    TipoPermiso = solicitud.TipoPermiso,
                    SolicitudPermisoId = solicitud.Id,
                },
                cancellationToken);
        }

        // Una sola unidad de trabajo: el cambio de estado y el alta del permiso se confirman juntos.
        await unidad.GuardarCambiosAsync(cancellationToken);

        return AResumen(solicitud);
    }

    public async Task<SolicitudPermisoResumen> RechazarAsync(
        int solicitudId,
        int administradorId,
        string motivo,
        CancellationToken cancellationToken = default)
    {
        var solicitud = await ObtenerRequeridaAsync(solicitudId, cancellationToken);

        // RF-GP-07: mismo chequeo de auto-resolución que en la aprobación.
        ExigirNoAutoResolucion(solicitud, administradorId);

        // RF-GP-04: el rechazo no se completa sin motivo (entrada externa validada, RD-07).
        if (string.IsNullOrWhiteSpace(motivo))
        {
            throw new ReglaNegocioExcepcion("El rechazo requiere un motivo.");
        }

        // RD-04/RF-GP-06: única fuente de verdad de las transiciones.
        TransicionesPermiso.Exigir(solicitud.Estado, EstadoSolicitudPermiso.Rechazada);

        solicitud.Estado = EstadoSolicitudPermiso.Rechazada;
        solicitud.ResueltaPorId = administradorId;
        solicitud.FechaResolucion = AhoraUtc();
        solicitud.MotivoRechazo = motivo.Trim();

        await unidad.GuardarCambiosAsync(cancellationToken);

        return AResumen(solicitud);
    }

    public Task<bool> TienePermisoAsync(
        int usuarioId,
        string tipoPermiso,
        CancellationToken cancellationToken = default)
        // RF-GP-05: consulta directa de permisos otorgados; demuestra el efecto de la aprobación.
        => unidad.PermisosActivos.ExistePermisoAsync(usuarioId, tipoPermiso, cancellationToken);

    private async Task<SolicitudPermiso> ObtenerRequeridaAsync(int solicitudId, CancellationToken cancellationToken)
        => await unidad.SolicitudesPermiso.BuscarPorIdAsync(solicitudId, cancellationToken)
           ?? throw new ReglaNegocioExcepcion("La solicitud no existe.", 404);

    private static void ExigirNoAutoResolucion(SolicitudPermiso solicitud, int administradorId)
    {
        if (solicitud.SolicitanteId == administradorId)
        {
            throw new ReglaNegocioExcepcion("No puedes resolver tu propia solicitud.", 409);
        }
    }

    private DateTime AhoraUtc() => timeProvider.GetUtcNow().UtcDateTime;

    private static SolicitudPermisoResumen AResumen(SolicitudPermiso solicitud)
        => new(
            solicitud.Id,
            solicitud.SolicitanteId,
            solicitud.TipoPermiso,
            solicitud.Estado.ToString(),
            solicitud.FechaCreacion,
            solicitud.ResueltaPorId,
            solicitud.FechaResolucion,
            solicitud.MotivoRechazo);
}
