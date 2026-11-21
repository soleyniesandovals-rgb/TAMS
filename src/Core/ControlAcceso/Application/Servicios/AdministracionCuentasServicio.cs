using Core.Auditoria.Application.Interfaces;
using Core.Auditoria.Domain.Enums;
using Core.ControlAcceso.Application.Interfaces;
using Core.ControlAcceso.Application.Modelos;
using Core.ControlAcceso.Application.Reglas;
using Core.ControlAcceso.Domain.Entidades;
using Core.ControlAcceso.Domain.Enums;
using Core.ControlAcceso.Domain.Excepciones;

namespace Core.ControlAcceso.Application.Servicios;

public class AdministracionCuentasServicio(
    ICoreUnidadDeTrabajo unidad,
    IRegistradorAuditoria registrador) : IAdministracionCuentasServicio
{
    public async Task<IReadOnlyList<UsuarioResumen>> ListarAsync(CancellationToken cancellationToken = default)
    {
        var usuarios = await unidad.Usuarios.ListarAsync(cancellationToken);

        // RF-CA-21: se proyecta a un DTO sin ContraseñaHash ni tokens.
        return usuarios
            .Select(u => new UsuarioResumen(u.Id, u.Nombre, u.Correo, u.Rol.ToString(), u.Activo))
            .ToList();
    }

    public async Task CambiarRolAsync(
        int usuarioId,
        string nuevoRol,
        int administradorId,
        CancellationToken cancellationToken = default)
    {
        // RD-02: la validación de la entrada externa se hace aquí, en Application, no en
        // el controlador. Se acepta cualquier combinación de mayúsculas, pero solo los
        // nombres de rol válidos: un número como "1" no es un rol. La regla vive en
        // ReglasRolUsuario, compartida con la lectura del claim del token. El mensaje es
        // controlado (RD-07, RD-08).
        var rol = ExigirRol(nuevoRol);

        // RF-CA-08: nadie cambia su propio rol, ni siquiera un Administrador. Esto
        // garantiza que el administrador que ejecuta la operación siga siéndolo, por
        // lo que el sistema nunca queda sin administradores.
        if (usuarioId == administradorId)
        {
            throw new ReglaNegocioExcepcion("No puedes cambiar tu propio rol.", 409);
        }

        var usuario = await ObtenerRequeridoAsync(usuarioId, cancellationToken);

        if (usuario.Rol == rol)
        {
            // CA-06: la operación no cambia nada, así que tampoco se registra.
            return;
        }

        var rolAnterior = usuario.Rol.ToString();
        usuario.Rol = rol;

        // El rol viaja en el JWT: se sube la versión de sesión para invalidar los
        // tokens vigentes del afectado, de modo que el nuevo rol surta efecto ya
        // (un Estandar no debe conservar privilegios de administrador hasta expirar).
        usuario.SesionVersion++;

        // RF-AUD-04/CA-06: el actor es el Administrador y el id es el del afectado.
        // Se agrega antes del único guardado (CA-12).
        await registrador.RegistrarAsync(
            administradorId,
            AccionAuditoria.CambiarRol,
            EntidadAuditoria.Usuario,
            usuario.Id,
            rolAnterior,
            rol.ToString(),
            cancellationToken);

        await unidad.GuardarCambiosAsync(cancellationToken);
    }

    public async Task DesactivarAsync(
        int usuarioId,
        int administradorId,
        CancellationToken cancellationToken = default)
    {
        // RF-CA-20: un Administrador no puede desactivarse a sí mismo.
        if (usuarioId == administradorId)
        {
            throw new ReglaNegocioExcepcion("No puedes desactivar tu propia cuenta.", 409);
        }

        var usuario = await ObtenerRequeridoAsync(usuarioId, cancellationToken);

        usuario.Activo = false;

        // RF-CA-20: se invalidan las sesiones abiertas con el mismo mecanismo del
        // logout (subir la versión de sesión). Además, al quedar inactivo no podrá
        // iniciar sesión (misma regla que RF-CA-15).
        usuario.SesionVersion++;

        // RF-AUD-04/CA-07: se registra solo tras pasar la validación de auto-desactivación
        // y antes del guardado (CA-12, CA-13).
        await registrador.RegistrarAsync(
            administradorId,
            AccionAuditoria.DesactivarUsuario,
            EntidadAuditoria.Usuario,
            usuario.Id,
            "Activo",
            "Inactivo",
            cancellationToken);

        await unidad.GuardarCambiosAsync(cancellationToken);
    }

    public async Task ReactivarAsync(
        int usuarioId,
        int administradorId,
        CancellationToken cancellationToken = default)
    {
        var usuario = await ObtenerRequeridoAsync(usuarioId, cancellationToken);

        usuario.Activo = true;

        // Al reactivar se limpia cualquier bloqueo por intentos fallidos para que la
        // cuenta pueda iniciar sesión de inmediato (RF-CA-19, RF-CA-20).
        usuario.IntentosFallidosConsecutivos = 0;
        usuario.BloqueadoHasta = null;

        // RF-AUD-04/CA-07: CA-07 exige el administrador que ejecuta la acción, recibido
        // desde el token y transmitido por el controlador.
        await registrador.RegistrarAsync(
            administradorId,
            AccionAuditoria.ReactivarUsuario,
            EntidadAuditoria.Usuario,
            usuario.Id,
            "Inactivo",
            "Activo",
            cancellationToken);

        await unidad.GuardarCambiosAsync(cancellationToken);
    }

    private async Task<Usuario> ObtenerRequeridoAsync(int usuarioId, CancellationToken cancellationToken)
        => await unidad.Usuarios.BuscarPorIdAsync(usuarioId, cancellationToken)
           ?? throw new ReglaNegocioExcepcion("El usuario no existe.", 404);

    /// <summary>
    /// Convierte el nombre de rol recibido en <see cref="RolUsuario"/> validándolo primero
    /// (RD-07, RD-02). Se aceptan mayúsculas mixtas y espacios sobrantes, pero solo los
    /// dos nombres del enum: <c>Enum.TryParse</c> también aceptaría "0" o "1", y eso no es
    /// un rol. Si no es válido, lanza <see cref="ReglaNegocioExcepcion"/> con el mensaje
    /// controlado que el controlador traduce a 400.
    /// </summary>
    private static RolUsuario ExigirRol(string? nombre)
        => ReglasRolUsuario.Obtener(nombre)
           ?? throw new ReglaNegocioExcepcion("El rol debe ser 'Administrador' o 'Estandar'.");
}