using Core.ControlAcceso.Domain.Entidades;

namespace Core.ControlAcceso.Application.Interfaces;

public interface ICodigosRecuperacionRepositorio
{
    Task<CodigoRecuperacion?> BuscarPorCodigoAsync(string codigo, CancellationToken cancellationToken = default);

    Task AgregarAsync(CodigoRecuperacion codigo, CancellationToken cancellationToken = default);

    /// <summary>Marca como usados todos los códigos pendientes de un usuario (RF-CA-10, RF-CA-13).</summary>
    Task<int> MarcarUsadosDelUsuarioAsync(int usuarioId, CancellationToken cancellationToken = default);
}
