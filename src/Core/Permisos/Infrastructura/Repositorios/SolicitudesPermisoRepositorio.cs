using Core.Permisos.Application.Interfaces;
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

    public async Task AgregarAsync(SolicitudPermiso solicitud, CancellationToken cancellationToken = default)
        => await context.SolicitudesPermiso.AddAsync(solicitud, cancellationToken);
}
