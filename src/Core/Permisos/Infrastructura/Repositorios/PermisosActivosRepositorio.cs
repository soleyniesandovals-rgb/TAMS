using Core.Permisos.Application.Interfaces;
using Core.Permisos.Domain.Entidades;
using Core.ControlAcceso.Infrastructura.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Core.Permisos.Infrastructura.Repositorios;

public class PermisosActivosRepositorio(CoreDbContext context) : IPermisosActivosRepositorio
{
    public Task<bool> ExistePermisoAsync(
        int usuarioId,
        string tipoPermiso,
        CancellationToken cancellationToken = default)
        => context.PermisosActivos
            .AnyAsync(p => p.UsuarioId == usuarioId && p.TipoPermiso == tipoPermiso, cancellationToken);

    public async Task AgregarAsync(PermisoActivo permiso, CancellationToken cancellationToken = default)
        => await context.PermisosActivos.AddAsync(permiso, cancellationToken);
}
