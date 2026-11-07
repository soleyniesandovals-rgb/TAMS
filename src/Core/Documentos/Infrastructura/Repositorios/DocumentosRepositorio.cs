using Core.ControlAcceso.Infrastructura.Persistence;
using Core.Documentos.Application.Interfaces;
using Core.Documentos.Domain.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Core.Documentos.Infrastructura.Repositorios;

public class DocumentosRepositorio(CoreDbContext context) : IDocumentosRepositorio
{
    public async Task AgregarAsync(Documento documento, CancellationToken cancellationToken = default)
        => await context.Documentos.AddAsync(documento, cancellationToken);

    public async Task<Documento?> BuscarPorIdAsync(int id, CancellationToken cancellationToken = default)
        => await context.Documentos.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Documento>> ListarPorUsuarioAsync(
        int usuarioId,
        CancellationToken cancellationToken = default)
        => await context.Documentos
            .AsNoTracking()
            .Where(d => d.UsuarioId == usuarioId)
            .OrderByDescending(d => d.FechaSubida)
            .ThenByDescending(d => d.Id)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Documento>> ListarTodosAsync(CancellationToken cancellationToken = default)
        => await context.Documentos
            .AsNoTracking()
            .OrderByDescending(d => d.FechaSubida)
            .ThenByDescending(d => d.Id)
            .ToListAsync(cancellationToken);
}
