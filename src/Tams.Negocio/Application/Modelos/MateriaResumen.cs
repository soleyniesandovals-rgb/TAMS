using Tams.Negocio.Domain.Enums;

namespace Tams.Negocio.Application.Modelos;

/// <summary>
/// Datos públicos de una materia. Los resultados de aprendizaje solo existen en
/// las materias técnicas y llegan ordenados por número.
/// </summary>
public record MateriaResumen(
    int Id,
    string Nombre,
    bool EsTecnica,
    TipoTecnico? TipoTecnico,
    int? CantidadRa,
    IReadOnlyList<ResultadoAprendizajeResumen> ResultadosAprendizaje);
