using Core.Permisos.Application.Interfaces;
using Core.Permisos.Domain.Entidades;
using Core.Permisos.Domain.Enums;

namespace Tests.Permisos.Dobles;

public class SolicitudesPermisoRepositorioFalso : ISolicitudesPermisoRepositorio
{
    private readonly List<SolicitudPermiso> _solicitudes = new();
    private int _proximoId = 1;

    public Task AgregarAsync(SolicitudPermiso solicitud, CancellationToken cancellationToken = default)
    {
        solicitud.Id = _proximoId++;
        _solicitudes.Add(solicitud);
        return Task.CompletedTask;
    }

    public Task<SolicitudPermiso?> BuscarPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var solicitud = _solicitudes.FirstOrDefault(s => s.Id == id);
        return Task.FromResult(solicitud);
    }

    public Task<IReadOnlyList<SolicitudPermiso>> ListarPorSolicitanteAsync(int solicitanteId, CancellationToken cancellationToken = default)
    {
        var resultado = _solicitudes
            .Where(s => s.SolicitanteId == solicitanteId)
            .ToList()
            .AsReadOnly();

        return Task.FromResult((IReadOnlyList<SolicitudPermiso>)resultado);
    }

    public Task<IReadOnlyList<SolicitudPermiso>> ListarPendientesAsync(CancellationToken cancellationToken = default)
    {
        var resultado = _solicitudes
            .Where(s => s.Estado == EstadoSolicitudPermiso.Pendiente)
            .ToList()
            .AsReadOnly();

        return Task.FromResult((IReadOnlyList<SolicitudPermiso>)resultado);
    }

    public void Limpiar()
    {
        _solicitudes.Clear();
        _proximoId = 1;
    }

    public List<SolicitudPermiso> ObtenerTodas()
    {
        return _solicitudes;
    }
}
