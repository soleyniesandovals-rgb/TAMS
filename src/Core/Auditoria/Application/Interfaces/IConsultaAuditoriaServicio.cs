using Core.Auditoria.Application.Modelos;

namespace Core.Auditoria.Application.Interfaces;

/// <summary>
/// Consulta del historial para el Administrador (RF-AUD-07, CA-15 a CA-18).
/// Los filtros llegan como texto, tal como vienen de la cadena de consulta (RD-07).
/// </summary>
public interface IConsultaAuditoriaServicio
{
    Task<IReadOnlyList<RegistroAuditoriaResumen>> ListarAsync(
        string? usuarioId,
        string? entidad,
        string? desde,
        string? hasta,
        CancellationToken cancellationToken = default);
}
