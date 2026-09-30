using Core.ControlAcceso.Application.Interfaces;
using Core.ControlAcceso.Infrastructura.Persistence;

namespace Core.ControlAcceso.Infrastructura.Repositorios;

public class CoreUnidadDeTrabajo(CoreDbContext context) : ICoreUnidadDeTrabajo
{
    public IUsuariosRepositorio Usuarios { get; } = new UsuariosRepositorio(context);

    public ITokenActivacionesRepositorio TokenActivaciones { get; } = new TokenActivacionesRepositorio(context);

    public ICorreosEnColaRepositorio CorreosEnCola { get; } = new CorreosEnColaRepositorio(context);

    public Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default)
        => context.SaveChangesAsync(cancellationToken);

    public void Dispose() => context.Dispose();
}