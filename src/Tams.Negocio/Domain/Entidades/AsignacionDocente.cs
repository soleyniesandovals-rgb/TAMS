using Tams.Negocio.Domain.Interfaces;

namespace Tams.Negocio.Domain.Entidades;

/// <summary>
/// Asignación de un docente (usuario del Core) a una materia y un curso.
/// El docente se referencia solo por <see cref="UsuarioId"/>: sin navegación ni FK hacia el Core (RD-03).
/// </summary>
public class AsignacionDocente : ICreacionAuditable
{
    public int Id { get; set; }

    /// <summary>Id del usuario docente en el módulo Core. Sin navegación ni FK (RD-03).</summary>
    public int UsuarioId { get; set; }

    public int MateriaId { get; set; }

    public int CursoId { get; set; }

    public int HorasSemanales { get; set; }

    public DateTime FechaCreacion { get; set; }

    public Materia Materia { get; set; } = null!;

    public Curso Curso { get; set; } = null!;

    public ICollection<BloqueHorario> BloquesHorarios { get; set; } = [];
}