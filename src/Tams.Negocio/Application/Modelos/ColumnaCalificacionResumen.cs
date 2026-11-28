using Tams.Negocio.Domain.Enums;

namespace Tams.Negocio.Application.Modelos;

/// <summary>
/// Columna de la tabla de calificaciones: P1..P4 en materias básicas o una por
/// resultado de aprendizaje en las técnicas. La etiqueta es "P1" o "RA1".
/// </summary>
public record ColumnaCalificacionResumen(
    TipoEvaluacion TipoEvaluacion,
    int Numero,
    string Etiqueta,
    int? Puntos,
    int? PuntajeMinimo);
