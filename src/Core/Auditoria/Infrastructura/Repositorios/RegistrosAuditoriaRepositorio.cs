using Core.Auditoria.Application.Interfaces;
using Core.Auditoria.Domain.Entidades;
using Core.ControlAcceso.Infrastructura.Persistence;

namespace Core.Auditoria.Infrastructura.Repositorios;

/// <summary>
/// Solo agrega (CA-14): no hay Actualizar, Editar, Eliminar ni Borrar. Tampoco guarda;
/// el registro se persiste con el mismo guardado de la unidad de trabajo de la
/// operación auditada (CA-12).
/// </summary>
public class RegistrosAuditoriaRepositorio(CoreDbContext context) : IRegistrosAuditoriaRepositorio
{
    public async Task AgregarAsync(RegistroAuditoria registro, CancellationToken cancellationToken = default)
        => await context.RegistrosAuditoria.AddAsync(registro, cancellationToken);
}
