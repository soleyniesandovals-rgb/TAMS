using Tams.Negocio.Domain.Enums;
using Tams.Negocio.Domain.Interfaces;

namespace Tams.Negocio.Domain.Entidades;

/// <summary>Materia del plan de estudios. Si es técnica, lleva tipo técnico y cantidad de RA.</summary>
public class Materia : ICreacionAuditable
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public bool EsTecnica { get; set; }

    /// <summary>Solo aplica cuando <see cref="EsTecnica"/> es true.</summary>
    public TipoTecnico? TipoTecnico { get; set; }

    /// <summary>Solo aplica cuando <see cref="EsTecnica"/> es true.</summary>
    public int? CantidadRA { get; set; }

    public DateTime FechaCreacion { get; set; }

    public ICollection<AsignacionDocente> AsignacionesDocentes { get; set; } = [];

    public ICollection<Calificacion> Calificaciones { get; set; } = [];
}