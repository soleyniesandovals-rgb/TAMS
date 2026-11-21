using Core.Auditoria.Application.Interfaces;
using Core.Auditoria.Infrastructura.Repositorios;
using Core.ControlAcceso.Application.Interfaces;
using Core.ControlAcceso.Infrastructura.Persistence;
using Core.Documentos.Application.Interfaces;
using Core.Documentos.Infrastructura.Repositorios;
using Core.Notificaciones.Application.Interfaces;
using Core.Notificaciones.Infrastructura.Repositorios;
using Core.Permisos.Application.Interfaces;
using Core.Permisos.Infrastructura.Repositorios;

namespace Core.ControlAcceso.Infrastructura.Repositorios;

public class CoreUnidadDeTrabajo(CoreDbContext context) : ICoreUnidadDeTrabajo
{
    public IUsuariosRepositorio Usuarios { get; } = new UsuariosRepositorio(context);

    public ITokenActivacionesRepositorio TokenActivaciones { get; } = new TokenActivacionesRepositorio(context);

    public ICorreosEnColaRepositorio CorreosEnCola { get; } = new CorreosEnColaRepositorio(context);

    public ICodigosRecuperacionRepositorio CodigosRecuperacion { get; } = new CodigosRecuperacionRepositorio(context);

    // RF-GP-01, RF-GP-05: repositorios de la pieza de permisos, sobre el mismo DbContext.
    public ISolicitudesPermisoRepositorio SolicitudesPermiso { get; } = new SolicitudesPermisoRepositorio(context);

    public IPermisosActivosRepositorio PermisosActivos { get; } = new PermisosActivosRepositorio(context);

    // RF-DOC-01: repositorio de documentos sobre el mismo DbContext.
    public IDocumentosRepositorio Documentos { get; } = new DocumentosRepositorio(context);

    // RF-NOT-04/RF-NOT-07: repositorios de notificaciones sobre el mismo DbContext.
    public INotificacionesRepositorio Notificaciones { get; } = new NotificacionesRepositorio(context);

    public IPreferenciasNotificacionRepositorio PreferenciasNotificacion { get; } = new PreferenciasNotificacionRepositorio(context);

    // RF-AUD-01/RF-AUD-06: el historial comparte el DbContext para que su registro
    // se guarde en la misma transacción que la operación auditada (CA-12).
    public IRegistrosAuditoriaRepositorio RegistrosAuditoria { get; } = new RegistrosAuditoriaRepositorio(context);

    public Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default)
        => context.SaveChangesAsync(cancellationToken);

    public async Task<T> EjecutarEnTransaccionAsync<T>(
        Func<CancellationToken, Task<T>> operacion,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operacion);

        // CA-12: una sola transacción para todas las escrituras del delegado. Si cualquiera
        // falla (incluido el registro de auditoría), se revierte todo antes de propagar.
        await using var transaccion = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var resultado = await operacion(cancellationToken);
            await transaccion.CommitAsync(cancellationToken);
            return resultado;
        }
        catch
        {
            await transaccion.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public void Dispose() => context.Dispose();
}