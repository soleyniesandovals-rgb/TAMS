using Core.ControlAcceso.Application.Interfaces;
using Core.ControlAcceso.Domain.Entidades;
using Core.ControlAcceso.Domain.Enums;
using Core.ControlAcceso.Infrastructura.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Core.ControlAcceso.Infrastructura.Repositorios;

public class CorreosEnColaRepositorio(CoreDbContext context) : ICorreosEnColaRepositorio
{
    public async Task AgregarAsync(CorreoEnCola correo, CancellationToken cancellationToken = default)
        => await context.CorreosEnCola.AddAsync(correo, cancellationToken);

    public async Task<IReadOnlyList<CorreoEnCola>> ObtenerPendientesAsync(CancellationToken cancellationToken = default)
        => await context.CorreosEnCola
            .AsNoTracking()
            .Where(c => c.Estado == EstadoCorreo.Pendiente)
            .OrderBy(c => c.Id)
            .ToListAsync(cancellationToken);

    /// <summary>
    /// UPDATE condicionado al estado actual (RF-NOT-09): pasa de Pendiente a Enviando
    /// solo si sigue Pendiente. Devuelve true si afectó al correo (reclamo exitoso).
    /// </summary>
    public async Task<bool> ReclamarAsync(int id, CancellationToken cancellationToken = default)
    {
        var filas = await context.CorreosEnCola
            .Where(c => c.Id == id && c.Estado == EstadoCorreo.Pendiente)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.Estado, EstadoCorreo.Enviando), cancellationToken);

        return filas > 0;
    }

    public async Task MarcarEnviadoAsync(int id, DateTime fechaEnvioUtc, CancellationToken cancellationToken = default)
        => await context.CorreosEnCola
            .Where(c => c.Id == id && c.Estado == EstadoCorreo.Enviando)
            .ExecuteUpdateAsync(s => s
                .SetProperty(c => c.Estado, EstadoCorreo.Enviado)
                .SetProperty(c => c.FechaEnvio, fechaEnvioUtc), cancellationToken);

    public async Task ReactivarAsync(int id, CancellationToken cancellationToken = default)
        => await context.CorreosEnCola
            .Where(c => c.Id == id && c.Estado == EstadoCorreo.Enviando)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.Estado, EstadoCorreo.Pendiente), cancellationToken);
}