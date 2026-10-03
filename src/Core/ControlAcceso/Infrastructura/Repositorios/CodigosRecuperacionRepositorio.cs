using Core.ControlAcceso.Application.Interfaces;
using Core.ControlAcceso.Domain.Entidades;
using Core.ControlAcceso.Infrastructura.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Core.ControlAcceso.Infrastructura.Repositorios;

public class CodigosRecuperacionRepositorio(CoreDbContext context) : ICodigosRecuperacionRepositorio
{
    public Task<CodigoRecuperacion?> BuscarPorCodigoAsync(string codigo, CancellationToken cancellationToken = default)
        => context.CodigosRecuperacion
            .Include(c => c.Usuario)
            .FirstOrDefaultAsync(c => c.Codigo == codigo, cancellationToken);

    public async Task AgregarAsync(CodigoRecuperacion codigo, CancellationToken cancellationToken = default)
        => await context.CodigosRecuperacion.AddAsync(codigo, cancellationToken);

    public async Task<int> MarcarUsadosDelUsuarioAsync(int usuarioId, CancellationToken cancellationToken = default)
        => await context.CodigosRecuperacion
            .Where(c => c.UsuarioId == usuarioId && !c.Usado)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.Usado, true), cancellationToken);
}
