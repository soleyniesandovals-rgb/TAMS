using Core.ControlAcceso.Application.Interfaces;

namespace Core.ControlAcceso.Application.Servicios;

public class SesionServicio(ICoreUnidadDeTrabajo unidad) : ISesionServicio
{
    public async Task<bool> ValidarSesionAsync(int usuarioId, int sesionVersion, CancellationToken cancellationToken = default)
    {
        var usuario = await unidad.Usuarios.BuscarPorIdAsync(usuarioId, cancellationToken);
        return usuario is not null && usuario.SesionVersion == sesionVersion;
    }

    public async Task CerrarSesionAsync(int usuarioId, CancellationToken cancellationToken = default)
    {
        var usuario = await unidad.Usuarios.BuscarPorIdAsync(usuarioId, cancellationToken);
        if (usuario is null)
        {
            return;
        }

        usuario.SesionVersion++;
        await unidad.GuardarCambiosAsync(cancellationToken);
    }
}