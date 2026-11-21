using Core.Auditoria.Application.Interfaces;
using Core.Auditoria.Domain.Enums;
using Core.ControlAcceso.Application.Interfaces;
using Core.ControlAcceso.Application.Reglas;
using Core.ControlAcceso.Domain.Excepciones;

namespace Core.ControlAcceso.Application.Servicios;

public class CambioContrasenaServicio(
    ICoreUnidadDeTrabajo unidad,
    IRegistradorAuditoria registrador,
    IContrasenaHasher hasher) : ICambioContrasenaServicio
{
    public async Task CambiarAsync(
        int usuarioId,
        string contrasenaActual,
        string contrasenaNueva,
        CancellationToken cancellationToken = default)
    {
        var usuario = await unidad.Usuarios.BuscarPorIdAsync(usuarioId, cancellationToken)
            ?? throw new ReglaNegocioExcepcion("La sesión no es válida.", 401);

        // RF-CA-22: la contraseña actual debe coincidir; si no, se rechaza sin cambios.
        if (!hasher.Verificar(contrasenaActual, usuario.ContraseñaHash))
        {
            throw new ReglaNegocioExcepcion("La contraseña actual no es correcta.");
        }

        // RF-CA-14: la nueva contraseña debe cumplir la política.
        PoliticaContrasena.Exigir(contrasenaNueva);

        // RF-CA-02: solo se guarda el hash BCrypt.
        usuario.ContraseñaHash = hasher.Hash(contrasenaNueva);

        // RF-CA-12: se invalidan las sesiones previas (el token actual también queda
        // invalidado, por lo que el usuario deberá iniciar sesión de nuevo).
        usuario.SesionVersion++;

        // RF-AUD-05/CA-10: el actor es el propio usuario con sesión. CA-11: ni la
        // contraseña nueva ni su hash se copian al registro (CA-12: antes del guardado).
        await registrador.RegistrarAsync(
            usuarioId,
            AccionAuditoria.CambiarContrasena,
            EntidadAuditoria.Usuario,
            usuarioId,
            null,
            "Contraseña cambiada por el propio usuario",
            cancellationToken);

        await unidad.GuardarCambiosAsync(cancellationToken);
    }
}
