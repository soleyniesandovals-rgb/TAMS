using Core.Permisos.Application.Interfaces;
using Core.Permisos.Application.Modelos;
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

    /// <summary>
    /// Doble en memoria del GROUP BY del repositorio real (RF-REP-01), añadido
    /// solo para satisfacer la interfaz; las pruebas del reporte usan EF InMemory.
    /// </summary>
    public Task<IReadOnlyList<ConteoSolicitudesPermiso>> ContarPorMesYEstadoAsync(
        DateTime desdeUtc,
        DateTime hastaExclusivoUtc,
        int? solicitanteId,
        CancellationToken cancellationToken = default)
    {
        var consulta = _solicitudes
            .Where(s => s.FechaCreacion >= desdeUtc && s.FechaCreacion < hastaExclusivoUtc);

        if (solicitanteId.HasValue)
        {
            consulta = consulta.Where(s => s.SolicitanteId == solicitanteId.Value);
        }

        var resultado = consulta
            .GroupBy(s => new { Anio = s.FechaCreacion.Year, Mes = s.FechaCreacion.Month, s.Estado })
            .Select(g => new ConteoSolicitudesPermiso(g.Key.Anio, g.Key.Mes, g.Key.Estado, g.Count()))
            .ToList()
            .AsReadOnly();

        return Task.FromResult((IReadOnlyList<ConteoSolicitudesPermiso>)resultado);
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
