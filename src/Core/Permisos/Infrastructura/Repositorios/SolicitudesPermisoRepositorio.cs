using Core.Permisos.Application.Interfaces;
using Core.Permisos.Application.Modelos;
using Core.Permisos.Domain.Entidades;
using Core.ControlAcceso.Infrastructura.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Core.Permisos.Infrastructura.Repositorios;

public class SolicitudesPermisoRepositorio(CoreDbContext context) : ISolicitudesPermisoRepositorio
{
    public Task<SolicitudPermiso?> BuscarPorIdAsync(int id, CancellationToken cancellationToken = default)
        => context.SolicitudesPermiso.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<IReadOnlyList<SolicitudPermiso>> ListarPorSolicitanteAsync(
        int solicitanteId,
        CancellationToken cancellationToken = default)
        => await context.SolicitudesPermiso
            .AsNoTracking()
            .Where(s => s.SolicitanteId == solicitanteId)
            .OrderByDescending(s => s.Id)
            .ToListAsync(cancellationToken);

    // RF-GP-02: este repositorio NO filtra por estado. Devuelve todas las solicitudes y el
    // filtro de "solo Pendiente" lo aplica el servicio, en un único lugar.
    public async Task<IReadOnlyList<SolicitudPermiso>> ListarTodasAsync(CancellationToken cancellationToken = default)
        => await context.SolicitudesPermiso
            .AsNoTracking()
            .OrderBy(s => s.Id)
            .ToListAsync(cancellationToken);

    /// <summary>
    /// RF-REP-01/C-20: el <c>GroupBy</c> se aplica ANTES de materializar, sobre el
    /// <c>IQueryable</c> de EF, de modo que SQL Server ejecuta un <c>GROUP BY</c> con
    /// <c>COUNT(*)</c>. Nunca se carga el listado de solicitudes para resumirlo en C#.
    /// RD-01: la consulta vive en el repositorio de la pieza dueña de los datos.
    /// </summary>
    public async Task<IReadOnlyList<ConteoSolicitudesPermiso>> ContarPorMesYEstadoAsync(
        DateTime desdeUtc,
        DateTime hastaExclusivoUtc,
        int? solicitanteId,
        CancellationToken cancellationToken = default)
    {
        // Extremos inclusivos: desde 00:00 UTC del primer día hasta ANTES de 00:00 UTC
        // del día siguiente, para contar también lo ocurrido a las 23:59 del último día (C-24).
        var consulta = context.SolicitudesPermiso
            .AsNoTracking()
            .Where(s => s.FechaCreacion >= desdeUtc && s.FechaCreacion < hastaExclusivoUtc);

        if (solicitanteId.HasValue)
        {
            consulta = consulta.Where(s => s.SolicitanteId == solicitanteId.Value);
        }

        // La proyección intermedia es un tipo anónimo: así EF traduce el GROUP BY
        // y el COUNT en ambos proveedores. El mapeo al DTO se hace sobre las filas
        // ya agregadas, nunca sobre el listado completo de solicitudes.
        var conteos = await consulta
            .GroupBy(s => new { Anio = s.FechaCreacion.Year, Mes = s.FechaCreacion.Month, s.Estado })
            .Select(g => new
            {
                g.Key.Anio,
                g.Key.Mes,
                g.Key.Estado,
                Cantidad = g.Count(),
            })
            .ToListAsync(cancellationToken);

        return conteos
            .Select(c => new ConteoSolicitudesPermiso(c.Anio, c.Mes, c.Estado, c.Cantidad))
            .OrderBy(c => c.Anio)
            .ThenBy(c => c.Mes)
            .ThenBy(c => c.Estado)
            .ToList();
    }

    public async Task AgregarAsync(SolicitudPermiso solicitud, CancellationToken cancellationToken = default)
        => await context.SolicitudesPermiso.AddAsync(solicitud, cancellationToken);
}
