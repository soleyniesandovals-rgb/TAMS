using Core.ControlAcceso.Application.Interfaces;
using Core.ControlAcceso.Domain.Excepciones;

namespace Core.ControlAcceso.Application.Servicios;

public class ActivacionCuentaServicio(
    ICoreUnidadDeTrabajo unidad,
    TimeProvider timeProvider) : IActivacionCuentaServicio
{
    public async Task ActivarAsync(string codigo, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            throw new ReglaNegocioExcepcion("El enlace de activación no es válido.");
        }

        var token = await unidad.TokenActivaciones.BuscarPorCodigoAsync(codigo, cancellationToken);

        // RF-CA-16: token inexistente, ya utilizado o vencido se rechaza y nada cambia.
        if (token is null)
        {
            throw new ReglaNegocioExcepcion("El enlace de activación no es válido.");
        }

        if (token.Usado)
        {
            throw new ReglaNegocioExcepcion("El enlace de activación ya fue utilizado.");
        }

        if (token.FechaVencimiento < timeProvider.GetUtcNow().UtcDateTime)
        {
            throw new ReglaNegocioExcepcion("El enlace de activación ha vencido.");
        }

        // La consulta del token incluye su usuario; al no guardar en los rechazos de arriba,
        // el estado de la base no cambia en los casos inválidos.
        token.Usuario.Activo = true;
        token.Usado = true;

        await unidad.GuardarCambiosAsync(cancellationToken);
    }
}