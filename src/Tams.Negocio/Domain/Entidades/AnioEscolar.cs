using Tams.Negocio.Domain.Interfaces;

namespace Tams.Negocio.Domain.Entidades;

/// <summary>
/// Año escolar (por ejemplo "2026-2027"). Solo uno puede estar activo a la vez
/// (regla garantizada por un índice único filtrado en <see cref="EsActivo"/>).
/// </summary>
public class AnioEscolar : ICreacionAuditable
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public DateOnly FechaInicio { get; set; }

    public DateOnly FechaFin { get; set; }

    public bool EsActivo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public ICollection<Curso> Cursos { get; set; } = [];

    public ICollection<Calificacion> Calificaciones { get; set; } = [];
}