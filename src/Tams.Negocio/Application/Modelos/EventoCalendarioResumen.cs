namespace Tams.Negocio.Application.Modelos;

/// <summary>
/// Evento propio de un año escolar (RF-NEG). El servicio fuerza <see cref="DateTimeKind.Utc"/>
/// en <see cref="FechaCreacion"/> para que la fecha se serialice con «Z» también cuando
/// viene de la base (RD-11).
/// </summary>
public record EventoCalendarioResumen(
    int Id,
    int AnioEscolarId,
    DateOnly Fecha,
    string Descripcion,
    DateTime FechaCreacion);
