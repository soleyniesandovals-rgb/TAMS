using Core.ControlAcceso.Application.Modelos;

namespace Core.ControlAcceso.Application.Interfaces;

/// <summary>
/// Consulta de solo lectura de usuarios para otros módulos (RF-NEG-02, RD-03).
/// Expone únicamente datos no sensibles, sin leer tablas del Core desde fuera.
/// </summary>
public interface IConsultaUsuariosServicio
{
    /// <summary>Obtiene los datos públicos de un usuario, o null si no existe.</summary>
    Task<UsuarioConsultaResumen?> ObtenerAsync(
        int usuarioId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene los datos públicos de varios usuarios en una sola consulta; omite los
    /// ids que no existen.
    /// </summary>
    Task<IReadOnlyList<UsuarioConsultaResumen>> ObtenerVariosAsync(
        IReadOnlyCollection<int> usuarioIds,
        CancellationToken cancellationToken = default);
}
