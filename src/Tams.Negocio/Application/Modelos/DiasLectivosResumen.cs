namespace Tams.Negocio.Application.Modelos;

/// <summary>
/// Resumen del cálculo de días lectivos de un año escolar (RD-03, RD-12).
/// </summary>
public record DiasLectivosResumen(
    int AnioEscolarId,
    string Nombre,
    DateOnly FechaInicio,
    DateOnly FechaFin,
    int DiasHabiles,
    IReadOnlyList<FeriadoDescontado> FeriadosDescontados,
    int DiasLectivos,
    string Fuente);

/// <summary>Feriado nacional que se descontó del calendario lectivo.</summary>
public record FeriadoDescontado(DateOnly Fecha, string Nombre);
