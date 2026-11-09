using Core.ControlAcceso.Domain.Excepciones;
using Microsoft.EntityFrameworkCore;
using Tams.Negocio.Application.Modelos;
using Tams.Negocio.Infrastructura.Persistence;

namespace Tams.Negocio.Application.Servicios;

/// <summary>
/// Consultas del calendario escolar y cálculo de días lectivos (RD-03, RD-12).
/// La lógica vive aquí, no en el controlador ni en el proveedor externo.
/// </summary>
public class CalendarioEscolarServicio(
    TamsDbContext context,
    IProveedorFeriados proveedorFeriados) : ICalendarioEscolarServicio
{
    private const string FuenteFeriados = "Calendarific (calendarific.com)";

    public async Task<IReadOnlyList<AnioEscolarResumen>> ListarAniosEscolaresAsync(
        CancellationToken cancellationToken = default)
    {
        var anios = await context.AniosEscolares
            .AsNoTracking()
            .OrderBy(a => a.FechaInicio)
            .ThenBy(a => a.Nombre)
            .Select(a => new AnioEscolarResumen(
                a.Id,
                a.Nombre,
                a.FechaInicio,
                a.FechaFin,
                a.EsActivo))
            .ToListAsync(cancellationToken);

        return anios;
    }

    public async Task<DiasLectivosResumen> CalcularDiasLectivosAsync(
        int anioEscolarId,
        CancellationToken cancellationToken = default)
    {
        var anioEscolar = await context.AniosEscolares
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == anioEscolarId, cancellationToken)
            ?? throw new ReglaNegocioExcepcion("El año escolar no existe.", 404);

        // RD-07: el rango de fechas se valida antes de consultar la fuente externa.
        if (anioEscolar.FechaFin < anioEscolar.FechaInicio)
        {
            throw new ReglaNegocioExcepcion(
                "La fecha de fin del año escolar no puede ser anterior a la fecha de inicio.");
        }

        var diasHabiles = ContarDiasHabiles(anioEscolar.FechaInicio, anioEscolar.FechaFin);

        // La API se consulta una vez por cada año calendario que toca el rango.
        var feriados = new List<(DateOnly Fecha, string Nombre)>();
        for (var anio = anioEscolar.FechaInicio.Year; anio <= anioEscolar.FechaFin.Year; anio++)
        {
            var feriadosDelAnio = await proveedorFeriados.ObtenerFeriadosNacionalesAsync(anio, cancellationToken);
            feriados.AddRange(feriadosDelAnio);
        }

        // Solo descuentan los feriados distintos que caen dentro del rango y entre semana.
        // Si dos feriados coinciden en la misma fecha, se descuenta una sola vez.
        var feriadosDescontados = feriados
            .Where(f => f.Fecha >= anioEscolar.FechaInicio
                        && f.Fecha <= anioEscolar.FechaFin
                        && EsDiaHabil(f.Fecha))
            .GroupBy(f => f.Fecha)
            .Select(g => new FeriadoDescontado(g.Key, g.First().Nombre))
            .OrderBy(f => f.Fecha)
            .ToList();

        return new DiasLectivosResumen(
            anioEscolar.Id,
            anioEscolar.Nombre,
            anioEscolar.FechaInicio,
            anioEscolar.FechaFin,
            diasHabiles,
            feriadosDescontados,
            diasHabiles - feriadosDescontados.Count,
            FuenteFeriados);
    }

    private static int ContarDiasHabiles(DateOnly inicio, DateOnly fin)
    {
        var diasHabiles = 0;

        for (var fecha = inicio; fecha <= fin; fecha = fecha.AddDays(1))
        {
            if (EsDiaHabil(fecha))
            {
                diasHabiles++;
            }
        }

        return diasHabiles;
    }

    private static bool EsDiaHabil(DateOnly fecha)
        => fecha.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday;
}
