using Core.ControlAcceso.Application.Interfaces;
using Core.ControlAcceso.Application.Modelos;
using Core.ControlAcceso.Domain.Entidades;

namespace Core.ControlAcceso.Application.Servicios;

/// <summary>
/// Consulta de solo lectura de usuarios para otros módulos (RF-NEG-02, RD-03).
/// Proyecta el usuario a un DTO sin contraseña, hash, tokens ni datos de sesión.
/// </summary>
public class ConsultaUsuariosServicio(IUsuariosRepositorio usuarios) : IConsultaUsuariosServicio
{
    public async Task<UsuarioConsultaResumen?> ObtenerAsync(
        int usuarioId,
        CancellationToken cancellationToken = default)
    {
        var usuario = await usuarios.BuscarPorIdAsync(usuarioId, cancellationToken);

        return usuario is null ? null : Resumir(usuario);
    }

    public async Task<IReadOnlyList<UsuarioConsultaResumen>> ObtenerVariosAsync(
        IReadOnlyCollection<int> usuarioIds,
        CancellationToken cancellationToken = default)
    {
        var ids = usuarioIds?.Distinct().ToList() ?? [];

        if (ids.Count == 0)
        {
            return [];
        }

        var encontrados = await usuarios.BuscarPorIdsAsync(ids, cancellationToken);

        return encontrados.Select(Resumir).ToList();
    }

    private static UsuarioConsultaResumen Resumir(Usuario usuario)
        => new(usuario.Id, usuario.Nombre, usuario.Correo, usuario.Rol.ToString(), usuario.Activo);
}
