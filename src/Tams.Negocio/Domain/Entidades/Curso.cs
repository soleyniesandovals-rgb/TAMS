using Tams.Negocio.Domain.Enums;
using Tams.Negocio.Domain.Interfaces;

namespace Tams.Negocio.Domain.Entidades;

/// <summary>Curso (grado y sección) de un centro. Puede ser técnico.</summary>
public class Curso : ICreacionAuditable
{
    public int Id { get; set; }

    public string Grado { get; set; } = string.Empty;

    public string Seccion { get; set; } = string.Empty;

    public bool EsTecnico { get; set; }

    /// <summary>Solo aplica cuando <see cref="EsTecnico"/> es true.</summary>
    public TipoTecnico? TipoTecnico { get; set; }

    public int AnioEscolarId { get; set; }

    public DateTime FechaCreacion { get; set; }

    public AnioEscolar AnioEscolar { get; set; } = null!;

    public ICollection<AsignacionDocente> AsignacionesDocentes { get; set; } = [];

    public ICollection<Estudiante> Estudiantes { get; set; } = [];
}