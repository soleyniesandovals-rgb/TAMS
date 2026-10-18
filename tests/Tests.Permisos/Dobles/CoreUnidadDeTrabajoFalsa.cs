using Core.ControlAcceso.Application.Interfaces;
using Core.Permisos.Application.Interfaces;

namespace Tests.Permisos.Dobles;

public class CoreUnidadDeTrabajoFalsa : ICoreUnidadDeTrabajo
{
    private readonly ISolicitudesPermisoRepositorio _solicitudesPermiso;
    private readonly IPermisosActivosRepositorio _permisosActivos;

    public CoreUnidadDeTrabajoFalsa(
        ISolicitudesPermisoRepositorio solicitudesPermiso,
        IPermisosActivosRepositorio permisosActivos)
    {
        _solicitudesPermiso = solicitudesPermiso;
        _permisosActivos = permisosActivos;
    }

    public IUsuariosRepositorio Usuarios => null!;
    public ITokenActivacionesRepositorio TokenActivaciones => null!;
    public ICorreosEnColaRepositorio CorreosEnCola => null!;
    public ICodigosRecuperacionRepositorio CodigosRecuperacion => null!;

    public ISolicitudesPermisoRepositorio SolicitudesPermiso => _solicitudesPermiso;
    public IPermisosActivosRepositorio PermisosActivos => _permisosActivos;

    public Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(0);
    }

    public void Dispose()
    {
    }
}
