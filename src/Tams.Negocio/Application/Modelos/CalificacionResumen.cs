using Tams.Negocio.Domain.Enums;

namespace Tams.Negocio.Application.Modelos;

/// <summary>
/// Vista de una calificación que devuelven las operaciones del módulo de negocio.
/// El estado viaja como texto para no exponer el enum fuera del dominio.
/// </summary>
public record CalificacionResumen(
    int Id,
    int EstudianteId,
    int MateriaId,
    int AnioEscolarId,
    TipoEvaluacion TipoEvaluacion,
    int? Periodo,
    int? NumeroRA,
    decimal PuntajeObtenido,
    decimal? PuntajeRecuperacion,
    string Estado,
    DateTime FechaCreacion);
