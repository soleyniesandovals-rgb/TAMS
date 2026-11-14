using Tams.Negocio.Domain.Interfaces;

namespace Tams.Negocio.Domain.Entidades;

/// <summary>
/// Resultado de aprendizaje de una materia. Se numera dentro de la materia y
/// aporta un puntaje entre 1 y 100 (RF-NEG-01).
/// </summary>
public class ResultadoAprendizaje : ICreacionAuditable
{
    public int Id { get; set; }

    public int MateriaId { get; set; }

    public int Numero { get; set; }

    /// <summary>Descripción opcional del resultado de aprendizaje.</summary>
    public string? Descripcion { get; set; }

    public int Puntos { get; set; }

    public DateTime FechaCreacion { get; set; }

    public Materia Materia { get; set; } = null!;
}
