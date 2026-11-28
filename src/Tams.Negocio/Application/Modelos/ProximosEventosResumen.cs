namespace Tams.Negocio.Application.Modelos;

/// <summary>
/// Eventos propios y feriados desde hoy hasta el fin del año escolar activo (RF-NEG).
/// Sin año activo el identificador es nulo, no hay feriados y la lista está vacía.
/// </summary>
public record ProximosEventosResumen(
    int? AnioEscolarId,
    bool FeriadosDisponibles,
    IReadOnlyList<ProximoEventoResumen> Elementos);

/// <summary>Entrada de la lista de próximos eventos: evento propio o feriado nacional.</summary>
public record ProximoEventoResumen(
    DateOnly Fecha,
    string Descripcion,
    string Tipo,
    int? EventoId);

/// <summary>Tipos posibles de <see cref="ProximoEventoResumen"/>.</summary>
public static class TipoProximoEvento
{
    public const string Evento = "Evento";

    public const string Feriado = "Feriado";
}
