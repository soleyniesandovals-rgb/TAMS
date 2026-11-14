using Tams.Negocio.Domain.Enums;
using Tams.Negocio.Domain.Interfaces;

namespace Tams.Negocio.Domain.Entidades;

/// <summary>
/// Asistencia de un estudiante a una clase (asignación docente) en una fecha.
/// Una fila representa un estudiante en una clase en una fecha (RF-NEG-01).
/// </summary>
public class Asistencia : ICreacionAuditable
{
    public int Id { get; set; }

    public int AsignacionDocenteId { get; set; }

    public int EstudianteId { get; set; }

    public DateOnly Fecha { get; set; }

    public EstadoAsistencia Estado { get; set; }

    /// <summary>Observación opcional sobre la asistencia.</summary>
    public string? Observacion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public AsignacionDocente AsignacionDocente { get; set; } = null!;

    public Estudiante Estudiante { get; set; } = null!;
}
