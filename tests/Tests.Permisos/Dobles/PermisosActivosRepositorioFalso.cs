using Core.Permisos.Application.Interfaces;
using Core.Permisos.Domain.Entidades;

namespace Tests.Permisos.Dobles;

public class PermisosActivosRepositorioFalso : IPermisosActivosRepositorio
{
    private readonly List<PermisoActivo> _permisosActivos = new();
    private int _proximoId = 1;

    public Task AgregarAsync(PermisoActivo permiso, CancellationToken cancellationToken = default)
    {
        permiso.Id = _proximoId++;
        _permisosActivos.Add(permiso);
        return Task.CompletedTask;
    }

    public Task<bool> ExistePermisoAsync(int usuarioId, string tipoPermiso, CancellationToken cancellationToken = default)
    {
        var existe = _permisosActivos.Any(p => p.UsuarioId == usuarioId && p.TipoPermiso == tipoPermiso);
        return Task.FromResult(existe);
    }

    public void Limpiar()
    {
        _permisosActivos.Clear();
        _proximoId = 1;
    }

    public List<PermisoActivo> ObtenerTodos()
    {
        return _permisosActivos;
    }
}
