using Core.ControlAcceso.Application.Interfaces;
using Core.ControlAcceso.Infrastructura.Persistence;
using Core.Documentos.Application.Interfaces;
using Core.Documentos.Infrastructura.Repositorios;
using Core.Permisos.Application.Interfaces;
using Core.Permisos.Infrastructura.Repositorios;

namespace Core.ControlAcceso.Infrastructura.Repositorios;

public class CoreUnidadDeTrabajo(CoreDbContext context) : ICoreUnidadDeTrabajo
{
    public IUsuariosRepositorio Usuarios { get; } = new UsuariosRepositorio(context);

    public ITokenActivacionesRepositorio TokenActivaciones { get; } = new TokenActivacionesRepositorio(context);

    public ICorreosEnColaRepositorio CorreosEnCola { get; } = new CorreosEnColaRepositorio(context);

    public ICodigosRecuperacionRepositorio CodigosRecuperacion { get; } = new CodigosRecuperacionRepositorio(context);

    // RF-GP-01, RF-GP-05: repositorios de la pieza de permisos, sobre el mismo DbContext.
    public ISolicitudesPermisoRepositorio SolicitudesPermiso { get; } = new SolicitudesPermisoRepositorio(context);

    public IPermisosActivosRepositorio PermisosActivos { get; } = new PermisosActivosRepositorio(context);

    // RF-DOC-01: repositorio de documentos sobre el mismo DbContext.
    public IDocumentosRepositorio Documentos { get; } = new DocumentosRepositorio(context);

    public Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default)
        => context.SaveChangesAsync(cancellationToken);

    public void Dispose() => context.Dispose();
}