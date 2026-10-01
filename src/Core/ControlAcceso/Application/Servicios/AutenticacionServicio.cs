using System.Net.Mail;
using Core.ControlAcceso.Application.Interfaces;
using Core.ControlAcceso.Application.Modelos;
using Core.ControlAcceso.Domain.Excepciones;

namespace Core.ControlAcceso.Application.Servicios;

public class AutenticacionServicio(
    ICoreUnidadDeTrabajo unidad,
    IContrasenaHasher hasher,
    TimeProvider timeProvider) : IAutenticacionServicio
{
    // RF-CA-19: umbral de fallos consecutivos y ventana del bloqueo temporal.
    private const int UmbralIntentosFallidos = 5;
    private static readonly TimeSpan DuracionBloqueo = TimeSpan.FromMinutes(15);

    // Hash de referencia para pagar el mismo costo de BCrypt cuando el correo no
    // existe: evita que la duración de la respuesta delate la existencia de la cuenta.
    private readonly string _hashDeReferencia = hasher.Hash("tams-consulta-inexistente");

    public async Task<ResultadoAutenticacion> AutenticarAsync(
        string correo,
        string contrasena,
        CancellationToken cancellationToken = default)
    {
        // RD-07: la entrada se valida antes de usarse.
        if (!EsCorreoValido(correo))
        {
            throw new ReglaNegocioExcepcion("El correo no es válido.");
        }

        var correoNormalizado = correo.Trim().ToLowerInvariant();
        var usuario = await unidad.Usuarios.BuscarPorCorreoAsync(correoNormalizado, cancellationToken);

        if (usuario is null)
        {
            // Anti-enumeración: un correo inexistente tarda lo mismo que uno existente
            // con contraseña incorrecta.
            hasher.Verificar(contrasena, _hashDeReferencia);
            return new ResultadoAutenticacion(EstadoAutenticacion.CredencialesInvalidas, null);
        }

        // RD-11: la hora viene del TimeProvider inyectado (UTC).
        var ahoraUtc = timeProvider.GetUtcNow().UtcDateTime;

        // RF-CA-19: mientras el bloqueo sea futuro, cualquier intento se rechaza, incluso
        // con la contraseña correcta. El bloqueo es un estado legítimo de informar, por lo
        // que no viola la "respuesta idéntica" de RF-CA-03/09/17 (que aplica a credenciales).
        if (usuario.BloqueadoHasta is DateTime bloqueadoHasta && bloqueadoHasta > ahoraUtc)
        {
            return new ResultadoAutenticacion(EstadoAutenticacion.CuentaBloqueada, usuario)
            {
                BloqueadoHasta = usuario.BloqueadoHasta,
            };
        }

        // RF-CA-02: se compara contra el hash; nunca se lee la contraseña en texto plano.
        if (!hasher.Verificar(contrasena, usuario.ContraseñaHash))
        {
            // RF-CA-19: cada intento fallido incrementa el contador.
            usuario.IntentosFallidosConsecutivos++;

            // RF-CA-19: al alcanzar el umbral se fija el fin del bloqueo temporal.
            if (usuario.IntentosFallidosConsecutivos >= UmbralIntentosFallidos)
            {
                usuario.BloqueadoHasta = ahoraUtc.Add(DuracionBloqueo);
            }

            await unidad.GuardarCambiosAsync(cancellationToken);
            return new ResultadoAutenticacion(EstadoAutenticacion.CredencialesInvalidas, null);
        }

        // RF-CA-15: una cuenta sin activar no puede iniciar sesión.
        if (!usuario.Activo)
        {
            return new ResultadoAutenticacion(EstadoAutenticacion.CuentaInactiva, usuario);
        }

        // RF-CA-19: un login exitoso limpia el contador de fallos y cualquier bloqueo vencido.
        usuario.IntentosFallidosConsecutivos = 0;
        usuario.BloqueadoHasta = null;
        await unidad.GuardarCambiosAsync(cancellationToken);

        return new ResultadoAutenticacion(EstadoAutenticacion.Exito, usuario);
    }

    private static bool EsCorreoValido(string correo)
        => MailAddress.TryCreate(correo, out var direccion) && direccion.Address == correo;
}