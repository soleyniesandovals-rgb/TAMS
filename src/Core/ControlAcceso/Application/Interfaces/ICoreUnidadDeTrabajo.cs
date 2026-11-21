using Core.Auditoria.Application.Interfaces;
using Core.Documentos.Application.Interfaces;
using Core.Notificaciones.Application.Interfaces;
using Core.Permisos.Application.Interfaces;

namespace Core.ControlAcceso.Application.Interfaces;

/// <summary>
/// Punto de entrada único para las operaciones del Core dentro de una misma unidad de trabajo.
/// Permite probar cada pieza sin levantar la aplicación (RD-12).
/// </summary>
public interface ICoreUnidadDeTrabajo : IDisposable
{
    IUsuariosRepositorio Usuarios { get; }

    ITokenActivacionesRepositorio TokenActivaciones { get; }

    ICorreosEnColaRepositorio CorreosEnCola { get; }

    ICodigosRecuperacionRepositorio CodigosRecuperacion { get; }

    ISolicitudesPermisoRepositorio SolicitudesPermiso { get; }

    IPermisosActivosRepositorio PermisosActivos { get; }

    IDocumentosRepositorio Documentos { get; }

    INotificacionesRepositorio Notificaciones { get; }

    IPreferenciasNotificacionRepositorio PreferenciasNotificacion { get; }

    // RF-AUD-01/RF-AUD-06: historial de solo agrego, sobre el mismo DbContext (CA-12, CA-14).
    IRegistrosAuditoriaRepositorio RegistrosAuditoria { get; }

    Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// RF-AUD-03/CA-12: ejecuta una operación cuyas escrituras deben confirmarse o
    /// revertirse juntas. El delegado puede guardar más de una vez: en la subida de
    /// documentos hace falta un primer guardado para que la base asigne el id que
    /// después viaja en el registro de auditoría. El registrador nunca guarda por sí mismo.
    /// </summary>
    Task<T> EjecutarEnTransaccionAsync<T>(
        Func<CancellationToken, Task<T>> operacion,
        CancellationToken cancellationToken = default);
}