using Tams.Negocio.Domain.Enums;
using Tams.Negocio.Domain.Interfaces;

namespace Tams.Negocio.Domain.Entidades;

/// <summary>
/// Calificación de un estudiante en una materia.
/// Puede ser por periodo o por RA. La transición de <see cref="Estado"/> se implementa en Fase 1 (S4).
/// </summary>
public class Calificacion : ICreacionAuditable
{
    public int Id { get; set; }

    public int EstudianteId { get; set; }

    public int MateriaId { get; set; }

    public int AnioEscolarId { get; set; }

    public TipoEvaluacion TipoEvaluacion { get; set; }

    /// <summary>Periodo evaluado cuando <see cref="TipoEvaluacion"/> es <see cref="TipoEvaluacion.Periodo"/>.</summary>
    public int? Periodo { get; set; }

    /// <summary>Número de RA evaluado cuando <see cref="TipoEvaluacion"/> es <see cref="TipoEvaluacion.RA"/>.</summary>
    public int? NumeroRA { get; set; }

    public decimal PuntajeObtenido { get; set; }

    public decimal? PuntajeRecuperacion { get; set; }

    public EstadoCalificacion Estado { get; set; }

    public DateTime FechaCreacion { get; set; }

    public Estudiante Estudiante { get; set; } = null!;

    public Materia Materia { get; set; } = null!;

    public AnioEscolar AnioEscolar { get; set; } = null!;
}