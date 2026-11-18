using Core.ControlAcceso.Application.Interfaces;
using Core.ControlAcceso.Domain.Entidades;
using Core.ControlAcceso.Domain.Enums;

namespace Tests.Permisos.Dobles;

/// <summary>Doble en memoria de <see cref="IUsuariosRepositorio"/> (RD-12).</summary>
public sealed class UsuariosRepositorioFalso(params Usuario[] usuarios) : IUsuariosRepositorio
{
    private readonly List<Usuario> _usuarios = [.. usuarios];

    public Task<Usuario?> BuscarPorCorreoAsync(string correo, CancellationToken cancellationToken = default)
        => Task.FromResult(_usuarios.FirstOrDefault(u => u.Correo == correo));

    public Task<Usuario?> BuscarPorIdAsync(int id, CancellationToken cancellationToken = default)
        => Task.FromResult(_usuarios.FirstOrDefault(u => u.Id == id));

    public Task<IReadOnlyList<Usuario>> BuscarPorIdsAsync(
        IReadOnlyCollection<int> ids,
        CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<Usuario>>(
            _usuarios.Where(u => ids.Contains(u.Id)).OrderBy(u => u.Id).ToList());

    public Task<IReadOnlyList<Usuario>> ListarAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<Usuario>>(_usuarios.OrderBy(u => u.Id).ToList());

    public Task<IReadOnlyList<Usuario>> ListarAdministradoresActivosAsync(
        CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<Usuario>>(
            _usuarios.Where(u => u.Rol == RolUsuario.Administrador && u.Activo).OrderBy(u => u.Id).ToList());

    public Task AgregarAsync(Usuario usuario, CancellationToken cancellationToken = default)
    {
        _usuarios.Add(usuario);
        return Task.CompletedTask;
    }
}
