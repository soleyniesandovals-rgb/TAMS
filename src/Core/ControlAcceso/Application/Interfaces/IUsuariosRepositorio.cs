using Core.ControlAcceso.Domain.Entidades;

namespace Core.ControlAcceso.Application.Interfaces;

public interface IUsuariosRepositorio
{
    Task<Usuario?> BuscarPorCorreoAsync(string correo, CancellationToken cancellationToken = default);

    Task<Usuario?> BuscarPorIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Devuelve todos los usuarios ordenados por Id (RF-CA-21).</summary>
    Task<IReadOnlyList<Usuario>> ListarAsync(CancellationToken cancellationToken = default);

    Task AgregarAsync(Usuario usuario, CancellationToken cancellationToken = default);
}