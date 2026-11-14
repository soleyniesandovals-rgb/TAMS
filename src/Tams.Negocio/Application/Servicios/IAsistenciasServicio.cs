using Tams.Negocio.Application.Modelos;

namespace Tams.Negocio.Application.Servicios;

/// <summary>
/// Consulta y guardado de la lista de asistencia de una asignación docente
/// (RF-NEG-02, RD-02, RD-11, RD-12). La lógica vive aquí, no en el controlador.
/// </summary>
public interface IAsistenciasServicio
{
    /// <summary>
    /// Obtiene la lista de una asignación y fecha con el estado guardado de cada
    /// estudiante del curso.
    /// </summary>
    Task<ListaAsistenciaResumen> ObtenerListaAsync(
        int usuarioId,
        int asignacionId,
        DateOnly fecha,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Guarda o corrige la lista completa de una asignación y fecha.
    /// </summary>
    Task<ListaAsistenciaResumen> GuardarListaAsync(
        int usuarioId,
        int asignacionId,
        DateOnly fecha,
        IReadOnlyList<RegistroAsistenciaEntrada>? registros,
        CancellationToken cancellationToken = default);
}
