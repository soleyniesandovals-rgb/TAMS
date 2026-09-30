using Core.ControlAcceso.Application.Interfaces;
using Core.ControlAcceso.Domain.Entidades;
using Core.ControlAcceso.Infrastructura.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Core.ControlAcceso.Infrastructura.Repositorios;

public class UsuariosRepositorio(CoreDbContext context) : IUsuariosRepositorio
{
    public Task<Usuario?> BuscarPorCorreoAsync(string correo, CancellationToken cancellationToken = default)
        => context.Usuarios.FirstOrDefaultAsync(u => u.Correo == correo, cancellationToken);

    public async Task<Usuario?> BuscarPorIdAsync(int id, CancellationToken cancellationToken = default)
        => await context.Usuarios.FindAsync([id], cancellationToken).AsTask();

    public async Task AgregarAsync(Usuario usuario, CancellationToken cancellationToken = default)
        => await context.Usuarios.AddAsync(usuario, cancellationToken);
}