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

    public CoreUnidadDeTrabajoFalsa(
        ISolicitudesPermisoRepositorio solicitudesPermiso,
        IPermisosActivosRepositorio permisosActivos,
        IUsuariosRepositorio? usuarios = null)
    {
        _solicitudesPermiso = solicitudesPermiso;
        _permisosActivos = permisosActivos;
        _usuarios = usuarios ?? new UsuariosRepositorioFalso();
    }

    public IUsuariosRepositorio Usuarios => _usuarios;
    public ITokenActivacionesRepositorio TokenActivaciones => null!;
    public ICorreosEnColaRepositorio CorreosEnCola => null!;
    public ICodigosRecuperacionRepositorio CodigosRecuperacion => null!;

    public ISolicitudesPermisoRepositorio SolicitudesPermiso => _solicitudesPermiso;
    public IPermisosActivosRepositorio PermisosActivos => _permisosActivos;
    public IDocumentosRepositorio Documentos => null!;
    public INotificacionesRepositorio Notificaciones => null!;
    public IPreferenciasNotificacionRepositorio PreferenciasNotificacion => null!;

    public Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(0);
    }

    public void Dispose()
    {
    }
}
