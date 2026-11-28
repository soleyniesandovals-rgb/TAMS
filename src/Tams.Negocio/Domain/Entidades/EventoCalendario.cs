using Tams.Negocio.Domain.Interfaces;

namespace Tams.Negocio.Domain.Entidades;

/// <summary>
/// Evento propio del calendario de un año escolar (RF-NEG). La descripción corresponde
/// al texto de la maqueta «Qué hay ese día». El usuario que lo crea se referencia solo
/// por <see cref="CreadoPorUsuarioId"/>: sin navegación ni FK hacia el Core (RD-03).
/// </summary>
public class EventoCalendario : ICreacionAuditable
{
    public int Id { get; set; }

    public int AnioEscolarId { get; set; }

    public DateOnly Fecha { get; set; }

    public string Descripcion { get; set; } = string.Empty;

    /// <summary>Id del usuario del Core que creó el evento. Sin navegación ni FK (RD-03).</summary>
    public int CreadoPorUsuarioId { get; set; }

    public DateTime FechaCreacion { get; set; }

    public AnioEscolar AnioEscolar { get; set; } = null!;
}
