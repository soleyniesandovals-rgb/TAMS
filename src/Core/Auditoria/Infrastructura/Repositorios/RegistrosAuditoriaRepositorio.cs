using Core.Auditoria.Application.Interfaces;
using Core.Auditoria.Application.Modelos;
using Core.Auditoria.Domain.Entidades;
using Core.ControlAcceso.Infrastructura.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Core.Auditoria.Infrastructura.Repositorios;

/// <summary>
/// Solo agrega y consulta (CA-14): no hay Actualizar, Editar, Eliminar ni Borrar. Tampoco guarda;
/// el registro se persiste con el mismo guardado de la unidad de trabajo de la
/// operación auditada (CA-12).
/// </summary>
public class RegistrosAuditoriaRepositorio(CoreDbContext context) : IRegistrosAuditoriaRepositorio
{
    public async Task AgregarAsync(RegistroAuditoria registro, CancellationToken cancellationToken = default)
        => await context.RegistrosAuditoria.AddAsync(registro, cancellationToken);

    public async Task<IReadOnlyList<RegistroAuditoria>> ConsultarAsync(
        FiltrosConsultaAuditoria filtros,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filtros);

        // CA-15/CA-16: los filtros se componen en la consulta, antes de materializar;
        // la lectura no sigue el cambio para no dejar entidades del historial en memoria.
        var consulta = context.RegistrosAuditoria.AsNoTracking().AsQueryable();

        if (filtros.UsuarioId is { } usuarioId)
        {
            consulta = consulta.Where(r => r.UsuarioId == usuarioId);
        }

        if (filtros.Entidad is { } entidad)
        {
            consulta = consulta.Where(r => r.Entidad == entidad);
        }

        if (filtros.DesdeUtc is { } desde)
        {
            consulta = consulta.Where(r => r.FechaHora >= desde);
        }

        if (filtros.HastaUtc is { } hasta)
        {
            // Extremo inclusivo: incluye cualquier hora del último día (CA-16).
            consulta = consulta.Where(r => r.FechaHora <= hasta);
        }

        return await consulta
            .OrderByDescending(r => r.FechaHora)
            .ThenByDescending(r => r.Id)
            .Take(FiltrosConsultaAuditoria.MaximoRegistros)
            .ToListAsync(cancellationToken);
    }
}
