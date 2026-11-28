using Core.ControlAcceso.Domain.Excepciones;
using Microsoft.EntityFrameworkCore;
using Tams.Negocio.Application.Excepciones;
using Tams.Negocio.Application.Modelos;
using Tams.Negocio.Domain;
using Tams.Negocio.Domain.Entidades;
using Tams.Negocio.Infrastructura.Persistence;

namespace Tams.Negocio.Application.Servicios;

/// <summary>
/// Eventos del calendario y próximos eventos (RF-NEG, RD-02, RD-07, RD-11).
/// La lógica vive aquí: el controlador solo traduce el resultado a HTTP. Los feriados
/// se consultan a <see cref="IProveedorFeriados"/> y nunca se persisten (RD-01).
/// </summary>
public class EventosCalendarioServicio(
    TamsDbContext context,
    IProveedorFeriados proveedorFeriados,
    TimeProvider timeProvider) : IEventosCalendarioServicio
{
    public async Task<IReadOnlyList<EventoCalendarioResumen>> ListarAsync(
        int anioEscolarId,
        CancellationToken cancellationToken = default)
    {
        await ObtenerAnioAsync(anioEscolarId, cancellationToken);

        var eventos = await context.EventosCalendario
            .AsNoTracking()
            .Where(e => e.AnioEscolarId == anioEscolarId)
            .OrderBy(e => e.Fecha)
            .ThenBy(e => e.Id)
            .ToListAsync(cancellationToken);

        return eventos.Select(CrearResumen).ToList();
    }

    public async Task<EventoCalendarioResumen> CrearAsync(
        int anioEscolarId,
        DateOnly fecha,
        string descripcion,
        int usuarioId,
        CancellationToken cancellationToken = default)
    {
        var anio = await ObtenerAnioAsync(anioEscolarId, cancellationToken);

        var descripcionNormalizada = ReglasEventoCalendario.NormalizarDescripcion(descripcion);

        if (!ReglasEventoCalendario.EsDescripcionValida(descripcionNormalizada))
        {
            throw new ReglaNegocioExcepcion(
                "La descripción del evento es obligatoria y no puede superar 200 caracteres.",
                400);
        }

        if (!ReglasEventoCalendario.EsFechaValida(fecha, anio.FechaInicio, anio.FechaFin))
        {
            throw new ReglaNegocioExcepcion(
                "La fecha del evento debe estar dentro del año escolar.",
                400);
        }

        var evento = new EventoCalendario
        {
            AnioEscolarId = anio.Id,
            Fecha = fecha,
            Descripcion = descripcionNormalizada,
            CreadoPorUsuarioId = usuarioId,
        };

        context.EventosCalendario.Add(evento);
        await context.SaveChangesAsync(cancellationToken);

        return CrearResumen(evento);
    }

    public async Task BorrarAsync(int id, CancellationToken cancellationToken = default)
    {
        var evento = await context.EventosCalendario
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken)
            ?? throw new ReglaNegocioExcepcion("El evento no existe.", 404);

        context.EventosCalendario.Remove(evento);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<ProximosEventosResumen> ProximosAsync(CancellationToken cancellationToken = default)
    {
        // RD-11: «hoy» se toma del TimeProvider inyectado, en UTC.
        var hoy = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);

        var anioActivo = await context.AniosEscolares
            .AsNoTracking()
            .Where(a => a.EsActivo)
            .FirstOrDefaultAsync(cancellationToken);

        if (anioActivo is null)
        {
            return new ProximosEventosResumen(null, false, []);
        }

        var fin = anioActivo.FechaFin;

        var eventos = await context.EventosCalendario
            .AsNoTracking()
            .Where(e => e.AnioEscolarId == anioActivo.Id && e.Fecha >= hoy && e.Fecha <= fin)
            .ToListAsync(cancellationToken);

        var elementos = eventos
            .Select(e => new ProximoEventoResumen(e.Fecha, e.Descripcion, TipoProximoEvento.Evento, e.Id))
            .ToList();

        var feriadosDisponibles = true;
        var feriados = new List<(DateOnly Fecha, string Nombre)>();

        try
        {
            // Una llamada por año calendario que toca el rango, igual que CalendarioEscolarServicio.
            for (var anio = hoy.Year; anio <= fin.Year; anio++)
            {
                feriados.AddRange(
                    await proveedorFeriados.ObtenerFeriadosNacionalesAsync(anio, cancellationToken));
            }

            elementos.AddRange(feriados
                .Where(f => f.Fecha >= hoy && f.Fecha <= fin)
                .GroupBy(f => (f.Fecha, f.Nombre))
                .Select(g => new ProximoEventoResumen(
                    g.Key.Fecha, g.Key.Nombre, TipoProximoEvento.Feriado, null)));
        }
        catch (ServicioFeriadosNoConfiguradoExcepcion)
        {
            feriadosDisponibles = false;
        }
        catch (ServicioFeriadosNoDisponibleExcepcion)
        {
            feriadosDisponibles = false;
        }

        var ordenados = elementos
            .OrderBy(e => e.Fecha)
            .ThenBy(e => e.Tipo == TipoProximoEvento.Feriado ? 0 : 1)
            .ThenBy(e => e.Descripcion, StringComparer.Ordinal)
            .ToList();

        return new ProximosEventosResumen(anioActivo.Id, feriadosDisponibles, ordenados);
    }

    private async Task<AnioEscolar> ObtenerAnioAsync(
        int anioEscolarId,
        CancellationToken cancellationToken)
        => await context.AniosEscolares
               .AsNoTracking()
               .FirstOrDefaultAsync(a => a.Id == anioEscolarId, cancellationToken)
           ?? throw new ReglaNegocioExcepcion("El año escolar no existe.", 404);

    private static EventoCalendarioResumen CrearResumen(EventoCalendario evento)
        => new(
            evento.Id,
            evento.AnioEscolarId,
            evento.Fecha,
            evento.Descripcion,
            DateTime.SpecifyKind(evento.FechaCreacion, DateTimeKind.Utc));
}
