using Tams.Negocio.Application.Modelos;

namespace Tams.Negocio.Application.Servicios;

/// <summary>
/// Operaciones de consulta del calendario escolar.
/// </summary>
public interface ICalendarioEscolarServicio
{
    /// <summary>Lista los años escolares registrados.</summary>
    Task<IReadOnlyList<AnioEscolarResumen>> ListarAniosEscolaresAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calcula los días lectivos de un año escolar descontando los feriados nacionales
    /// que caen entre semana dentro de su rango de fechas.
    /// </summary>
    Task<DiasLectivosResumen> CalcularDiasLectivosAsync(
        int anioEscolarId,
        CancellationToken cancellationToken = default);
}
