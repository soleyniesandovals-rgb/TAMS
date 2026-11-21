using Core.Auditoria.Application.Interfaces;
using Core.Auditoria.Application.Modelos;
using Core.Auditoria.Domain.Entidades;
using Core.ControlAcceso.Application.Interfaces;
using Core.Documentos.Application.Interfaces;
using Core.Notificaciones.Application.Interfaces;
using Core.Permisos.Application.Interfaces;

namespace Tests.Core.Dobles;

/// <summary>
/// Unidad de trabajo en memoria para probar el registrador sin SQL Server (RD-12).
/// Cuenta los guardados para comprobar que el registrador no guarda por sí mismo (CA-12).
/// </summary>
public sealed class UnidadDeTrabajoAuditoriaFalsa : ICoreUnidadDeTrabajo
{
    public List<RegistroAuditoria> Registros { get; } = [];

    public int Guardados { get; private set; }

    public IRegistrosAuditoriaRepositorio RegistrosAuditoria { get; }

    public UnidadDeTrabajoAuditoriaFalsa()
    {
        RegistrosAuditoria = new RegistrosAuditoriaFalso(Registros);
    }

    public IUsuariosRepositorio Usuarios => null!;

    public ITokenActivacionesRepositorio TokenActivaciones => null!;

    public ICorreosEnColaRepositorio CorreosEnCola => null!;

    public ICodigosRecuperacionRepositorio CodigosRecuperacion => null!;

    public ISolicitudesPermisoRepositorio SolicitudesPermiso => null!;

    public IPermisosActivosRepositorio PermisosActivos => null!;

    public IDocumentosRepositorio Documentos => null!;

    public INotificacionesRepositorio Notificaciones => null!;

    public IPreferenciasNotificacionRepositorio PreferenciasNotificacion => null!;

    public Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default)
    {
        Guardados++;
        return Task.FromResult(0);
    }

    public Task<T> EjecutarEnTransaccionAsync<T>(
        Func<CancellationToken, Task<T>> operacion,
        CancellationToken cancellationToken = default)
        => operacion(cancellationToken);

    public void Dispose()
    {
    }

    private sealed class RegistrosAuditoriaFalso(List<RegistroAuditoria> registros) : IRegistrosAuditoriaRepositorio
    {
        public Task AgregarAsync(RegistroAuditoria registro, CancellationToken cancellationToken = default)
        {
            registros.Add(registro);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<RegistroAuditoria>> ConsultarAsync(
            FiltrosConsultaAuditoria filtros,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException("Este doble solo registra; la consulta usa EF Core InMemory (RD-12).");
    }
}
