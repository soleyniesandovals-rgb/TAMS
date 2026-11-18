using Core.ControlAcceso.Domain.Entidades;

namespace Core.ControlAcceso.Application.Interfaces;

public interface IUsuariosRepositorio
{
    Task<Usuario?> BuscarPorCorreoAsync(string correo, CancellationToken cancellationToken = default);

    Task<Usuario?> BuscarPorIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca varios usuarios por Id en una sola consulta, ordenados por Id.
    /// Devuelve solo los que existen (RF-NEG-02, RD-03).
    /// </summary>
    Task<IReadOnlyList<Usuario>> BuscarPorIdsAsync(
        IReadOnlyCollection<int> ids,
        CancellationToken cancellationToken = default);

    /// <summary>Devuelve todos los usuarios ordenados por Id (RF-CA-21).</summary>
    Task<IReadOnlyList<Usuario>> ListarAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Devuelve los Administradores con la cuenta activa, ordenados por Id (RF-NOT-01).
    /// Se añade sin modificar las consultas existentes.
    /// </summary>
    Task<IReadOnlyList<Usuario>> ListarAdministradoresActivosAsync(CancellationToken cancellationToken = default);

    Task AgregarAsync(Usuario usuario, CancellationToken cancellationToken = default);
}