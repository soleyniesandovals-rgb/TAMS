using Tams.Negocio.Application.Modelos;

namespace Tams.Negocio.Application.Servicios;

/// <summary>
/// Alta, listado y borrado de eventos propios del calendario, y consulta de los
/// próximos eventos junto con los feriados nacionales (RF-NEG, RD-02).
/// </summary>
public interface IEventosCalendarioServicio
{
    /// <summary>Lista los eventos de un año escolar, ordenados por fecha y luego por id.</summary>
    Task<IReadOnlyList<EventoCalendarioResumen>> ListarAsync(
        int anioEscolarId,
        CancellationToken cancellationToken = default);

    /// <summary>Crea un evento propio en un año escolar.</summary>
    Task<EventoCalendarioResumen> CrearAsync(
        int anioEscolarId,
        DateOnly fecha,
        string descripcion,
        int usuarioId,
        CancellationToken cancellationToken = default);

    /// <summary>Borra un evento propio. Los feriados no se guardan ni se borran.</summary>
    Task BorrarAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Devuelve eventos propios y feriados desde hoy hasta el fin del año activo.</summary>
    Task<ProximosEventosResumen> ProximosAsync(CancellationToken cancellationToken = default);
}
