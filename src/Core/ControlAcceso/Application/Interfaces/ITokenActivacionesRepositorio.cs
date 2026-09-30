using Core.ControlAcceso.Domain.Entidades;

namespace Core.ControlAcceso.Application.Interfaces;

public interface ITokenActivacionesRepositorio
{
    Task<TokenActivacion?> BuscarPorCodigoAsync(string codigo, CancellationToken cancellationToken = default);

    Task AgregarAsync(TokenActivacion token, CancellationToken cancellationToken = default);

    /// <summary>Marca como usados todos los tokens pendientes de un usuario (RF-CA-17).</summary>
    Task<int> MarcarUsadosDelUsuarioAsync(int usuarioId, CancellationToken cancellationToken = default);
}