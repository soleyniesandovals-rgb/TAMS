namespace Tams.Negocio.Application.Modelos;

/// <summary>
/// Datos públicos de un año escolar para el listado del módulo de negocio.
/// </summary>
public record AnioEscolarResumen(
    int Id,
    string Nombre,
    DateOnly FechaInicio,
    DateOnly FechaFin,
    bool EsActivo);
