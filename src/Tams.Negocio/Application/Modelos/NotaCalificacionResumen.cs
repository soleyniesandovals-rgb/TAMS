namespace Tams.Negocio.Application.Modelos;

/// <summary>
/// Nota existente en una columna de la tabla. El estado viaja como texto y el
/// puntaje efectivo ya aplica la recuperación mediante <c>ReglasAcumulado</c>.
/// </summary>
public record NotaCalificacionResumen(
    int Numero,
    int CalificacionId,
    decimal PuntajeObtenido,
    decimal? PuntajeRecuperacion,
    decimal PuntajeEfectivo,
    string Estado);
