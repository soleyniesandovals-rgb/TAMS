using Tams.Negocio.Domain.Interfaces;

namespace Tams.Negocio.Domain.Entidades;

/// <summary>Estudiante perteneciente a un curso.</summary>
public class Estudiante : ICreacionAuditable
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Apellido { get; set; } = string.Empty;

    /// <summary>Cédula / NUI del acta de nacimiento: 11 dígitos sin guiones.
    /// Dato personal: solo lo ve el Administrador.</summary>
    public string? Cedula { get; set; }

    public int CursoId { get; set; }

    public DateTime FechaCreacion { get; set; }

    public Curso Curso { get; set; } = null!;

    public ICollection<Calificacion> Calificaciones { get; set; } = [];
}