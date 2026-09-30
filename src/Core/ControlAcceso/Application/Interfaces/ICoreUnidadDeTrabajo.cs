namespace Core.ControlAcceso.Application.Interfaces;

/// <summary>
/// Punto de entrada único para las operaciones del Core dentro de una misma unidad de trabajo.
/// Permite probar cada pieza sin levantar la aplicación (RD-12).
/// </summary>
public interface ICoreUnidadDeTrabajo : IDisposable
{
    IUsuariosRepositorio Usuarios { get; }

    ITokenActivacionesRepositorio TokenActivaciones { get; }

    ICorreosEnColaRepositorio CorreosEnCola { get; }

    Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default);
}