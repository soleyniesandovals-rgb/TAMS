using Core.ControlAcceso.Application.Interfaces;
using Core.ControlAcceso.Domain.Entidades;
using Core.ControlAcceso.Infrastructura.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Core.ControlAcceso.Infrastructura.Repositorios;

public class TokenActivacionesRepositorio(CoreDbContext context) : ITokenActivacionesRepositorio
{
    public Task<TokenActivacion?> BuscarPorCodigoAsync(string codigo, CancellationToken cancellationToken = default)
        => context.TokenActivaciones
            .Include(t => t.Usuario)
            .FirstOrDefaultAsync(t => t.Codigo == codigo, cancellationToken);

    public async Task AgregarAsync(TokenActivacion token, CancellationToken cancellationToken = default)
        => await context.TokenActivaciones.AddAsync(token, cancellationToken);

    public async Task<int> MarcarUsadosDelUsuarioAsync(int usuarioId, CancellationToken cancellationToken = default)
        => await context.TokenActivaciones
            .Where(t => t.UsuarioId == usuarioId && !t.Usado)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.Usado, true), cancellationToken);
}