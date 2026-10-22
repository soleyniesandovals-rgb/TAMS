using Core.Permisos.Application.Interfaces;
using Core.Permisos.Domain.Entidades;

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

    /// <summary>
    /// Devuelve TODAS las solicitudes que contiene, sin filtrar por estado: el doble es fiel
    /// a un almacen de datos, no a una regla de negocio. El filtro de "solo Pendiente" (RF-GP-02)
    /// lo aplica el servicio, que es el unico lugar donde debe estar.
    /// </summary>
    public Task<IReadOnlyList<SolicitudPermiso>> ListarTodasAsync(CancellationToken cancellationToken = default)
    {
        var resultado = _solicitudes
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
