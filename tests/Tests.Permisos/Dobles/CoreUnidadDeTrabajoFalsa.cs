using Core.Auditoria.Application.Interfaces;
using Core.Auditoria.Application.Modelos;
using Core.Auditoria.Domain.Entidades;
using Core.ControlAcceso.Application.Interfaces;
using Core.Documentos.Application.Interfaces;
using Core.Notificaciones.Application.Interfaces;
using Core.Permisos.Application.Interfaces;

namespace Tests.Permisos.Dobles;

public class CoreUnidadDeTrabajoFalsa : ICoreUnidadDeTrabajo
{
    private readonly ISolicitudesPermisoRepositorio _solicitudesPermiso;
    private readonly IPermisosActivosRepositorio _permisosActivos;
    private readonly IUsuariosRepositorio _usuarios;
    private readonly List<int> _registrosPorGuardado = [];

    public CoreUnidadDeTrabajoFalsa(
        ISolicitudesPermisoRepositorio solicitudesPermiso,
        IPermisosActivosRepositorio permisosActivos,
        IUsuariosRepositorio? usuarios = null)
    {
        _solicitudesPermiso = solicitudesPermiso;
        _permisosActivos = permisosActivos;
        _usuarios = usuarios ?? new UsuariosRepositorioFalso();
        RegistrosAuditoria = new RegistrosAuditoriaFalso(Registros);
    }

    /// <summary>Registros agregados por el registrador, sin guardado propio (CA-12).</summary>
    public List<RegistroAuditoria> Registros { get; } = [];

    /// <summary>Cantidad de registros presentes en cada guardado (CA-12).</summary>
    public IReadOnlyList<int> RegistrosPorGuardado => _registrosPorGuardado;

    public int Guardados { get; private set; }

    public IUsuariosRepositorio Usuarios => _usuarios;
    public ITokenActivacionesRepositorio TokenActivaciones => null!;
    public ICorreosEnColaRepositorio CorreosEnCola => null!;
    public ICodigosRecuperacionRepositorio CodigosRecuperacion => null!;

    public ISolicitudesPermisoRepositorio SolicitudesPermiso => _solicitudesPermiso;
    public IPermisosActivosRepositorio PermisosActivos => _permisosActivos;
    public IDocumentosRepositorio Documentos => null!;
    public INotificacionesRepositorio Notificaciones => null!;
    public IPreferenciasNotificacionRepositorio PreferenciasNotificacion => null!;
    public IRegistrosAuditoriaRepositorio RegistrosAuditoria { get; }

    public Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default)
    {
        Guardados++;
        _registrosPorGuardado.Add(Registros.Count);
        return Task.FromResult(0);
    }

    public Task<T> EjecutarEnTransaccionAsync<T>(
        Func<CancellationToken, Task<T>> operacion,
        CancellationToken cancellationToken = default)
        => operacion(cancellationToken);

    public void Dispose()
    {
    }

    private sealed class RegistrosAuditoriaFalso(List<RegistroAuditoria> registros) : IRegistrosAuditoriaRepositorio
    {
        public Task AgregarAsync(RegistroAuditoria registro, CancellationToken cancellationToken = default)
        {
            registros.Add(registro);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<RegistroAuditoria>> ConsultarAsync(
            FiltrosConsultaAuditoria filtros,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException("Este doble solo registra; la consulta usa EF Core InMemory (RD-12).");
    }
}
