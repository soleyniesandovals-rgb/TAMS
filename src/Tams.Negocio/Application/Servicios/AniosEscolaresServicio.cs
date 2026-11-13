using Core.ControlAcceso.Domain.Excepciones;
using Microsoft.EntityFrameworkCore;
using Tams.Negocio.Application.Modelos;
using Tams.Negocio.Domain.Entidades;
using Tams.Negocio.Infrastructura.Persistence;

namespace Tams.Negocio.Application.Servicios;

/// <summary>
/// Alta y activación de años escolares (RF-NEG-02, RD-02, RD-12).
/// La creación y la activación viven aquí; el cálculo de días lectivos sigue en
/// <see cref="CalendarioEscolarServicio"/> (RD-01).
/// </summary>
public class AniosEscolaresServicio(TamsDbContext context) : IAniosEscolaresServicio
{
    private const int LongitudMaximaNombre = 30;

    public async Task<AnioEscolarResumen> CrearAsync(
        string nombre,
        DateOnly fechaInicio,
        DateOnly fechaFin,
        CancellationToken cancellationToken = default)
    {
        var nombreNormalizado = nombre?.Trim() ?? string.Empty;

        if (nombreNormalizado.Length == 0 || nombreNormalizado.Length > LongitudMaximaNombre)
        {
            throw new ReglaNegocioExcepcion(
                "El nombre del año escolar es obligatorio y admite hasta 30 caracteres.",
                400);
        }

        if (fechaFin <= fechaInicio)
        {
            throw new ReglaNegocioExcepcion(
                "La fecha de fin debe ser posterior a la fecha de inicio.",
                400);
        }

        var yaExiste = await context.AniosEscolares
            .AsNoTracking()
            .AnyAsync(a => a.Nombre == nombreNormalizado, cancellationToken);

        if (yaExiste)
        {
            throw new ReglaNegocioExcepcion("Ya existe un año escolar con ese nombre.", 409);
        }

        // Un año escolar nuevo nunca nace activo: la activación es una operación aparte.
        var anio = new AnioEscolar
        {
            Nombre = nombreNormalizado,
            FechaInicio = fechaInicio,
            FechaFin = fechaFin,
            EsActivo = false,
        };

        context.AniosEscolares.Add(anio);
        await context.SaveChangesAsync(cancellationToken);

        return Resumir(anio);
    }

    public async Task<AnioEscolarResumen> ActivarAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var anio = await context.AniosEscolares
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken)
            ?? throw new ReglaNegocioExcepcion("El año escolar no existe.", 404);

        if (anio.EsActivo)
        {
            return Resumir(anio);
        }

        await using var transaccion = await context.Database.BeginTransactionAsync(cancellationToken);

        // Dos SaveChanges dentro de una transacción: primero se desactivan los años
        // activos y luego se activa el pedido. Con un solo SaveChanges, SQL Server
        // podría aplicar la activación antes que la desactivación y el índice único
        // filtrado [EsActivo] = 1 rechazaría el cambio.
        var activos = await context.AniosEscolares
            .Where(a => a.EsActivo)
            .ToListAsync(cancellationToken);

        foreach (var activo in activos)
        {
            activo.EsActivo = false;
        }

        await context.SaveChangesAsync(cancellationToken);

        anio.EsActivo = true;
        await context.SaveChangesAsync(cancellationToken);

        await transaccion.CommitAsync(cancellationToken);

        return Resumir(anio);
    }

    private static AnioEscolarResumen Resumir(AnioEscolar anio)
        => new(anio.Id, anio.Nombre, anio.FechaInicio, anio.FechaFin, anio.EsActivo);
}
