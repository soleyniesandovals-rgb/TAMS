using Tams.Negocio.Domain.Enums;
using Tams.Negocio.Domain.Interfaces;

namespace Tams.Negocio.Domain.Entidades;

/// <summary>Bloque horario dentro de la asignación de un docente. Puede estar bloqueado.</summary>
public class BloqueHorario : ICreacionAuditable
{
    public int Id { get; set; }

    public int AsignacionDocenteId { get; set; }

    public DiaSemana DiaSemana { get; set; }

    public TimeOnly HoraInicio { get; set; }

    public TimeOnly HoraFin { get; set; }

    public bool Bloqueado { get; set; }

    public DateTime FechaCreacion { get; set; }

    public AsignacionDocente AsignacionDocente { get; set; } = null!;
}