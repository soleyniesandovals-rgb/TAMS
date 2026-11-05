using Core.Documentos.Domain.Entidades;

namespace Core.Documentos.Application.Interfaces;

/// <summary>Acceso a los documentos persistidos (RF-DOC-01 a RF-DOC-04).</summary>
public interface IDocumentosRepositorio
{
    Task AgregarAsync(Documento documento, CancellationToken cancellationToken = default);

    Task<Documento?> BuscarPorIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Documento>> ListarPorUsuarioAsync(int usuarioId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Documento>> ListarTodosAsync(CancellationToken cancellationToken = default);
}
