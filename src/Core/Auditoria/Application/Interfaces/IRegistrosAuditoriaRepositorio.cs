using Core.Auditoria.Application.Modelos;
using Core.Auditoria.Domain.Entidades;

namespace Core.Auditoria.Application.Interfaces;

/// <summary>
/// Repositorio de solo agrego y consulta del historial (RF-AUD-01, RF-AUD-06, RF-AUD-07,
/// CA-14): no expone operaciones de editar ni borrar.
/// </summary>
public interface IRegistrosAuditoriaRepositorio
{
    Task AgregarAsync(RegistroAuditoria registro, CancellationToken cancellationToken = default);

    /// <summary>
    /// Consulta el historial aplicando los filtros en la base sobre <c>IQueryable</c>,
    /// ordenado de más reciente a más antiguo (FechaHora y después Id, descendentes)
    /// y con un máximo de <see cref="FiltrosConsultaAuditoria.MaximoRegistros"/> filas
    /// (RF-AUD-07, CA-15, CA-16). No modifica ni elimina nada (CA-14).
    /// </summary>
    Task<IReadOnlyList<RegistroAuditoria>> ConsultarAsync(
        FiltrosConsultaAuditoria filtros,
        CancellationToken cancellationToken = default);
}
