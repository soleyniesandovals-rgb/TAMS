using Core.ControlAcceso.Application.Interfaces;
using Core.ControlAcceso.Application.Modelos;
using Core.ControlAcceso.Domain.Entidades;
using Core.ControlAcceso.Domain.Enums;
using Core.ControlAcceso.Domain.Excepciones;

namespace Core.ControlAcceso.Application.Servicios;

public class AdministracionCuentasServicio(ICoreUnidadDeTrabajo unidad) : IAdministracionCuentasServicio
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
        RolUsuario nuevoRol,
        int administradorId,
        CancellationToken cancellationToken = default)
    {
        // RF-CA-08: nadie cambia su propio rol, ni siquiera un Administrador. Esto
        // garantiza que el administrador que ejecuta la operación siga siéndolo, por
        // lo que el sistema nunca queda sin administradores.
        if (usuarioId == administradorId)
        {
            throw new ReglaNegocioExcepcion("No puedes cambiar tu propio rol.", 409);
        }

        var usuario = await ObtenerRequeridoAsync(usuarioId, cancellationToken);

        if (usuario.Rol == nuevoRol)
        {
            return;
        }

        usuario.Rol = nuevoRol;

        // El rol viaja en el JWT: se sube la versión de sesión para invalidar los
        // tokens vigentes del afectado, de modo que el nuevo rol surta efecto ya
        // (un Estandar no debe conservar privilegios de administrador hasta expirar).
        usuario.SesionVersion++;
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
        await unidad.GuardarCambiosAsync(cancellationToken);
    }

    public async Task ReactivarAsync(int usuarioId, CancellationToken cancellationToken = default)
    {
        var usuario = await ObtenerRequeridoAsync(usuarioId, cancellationToken);

        usuario.Activo = true;

        // Al reactivar se limpia cualquier bloqueo por intentos fallidos para que la
        // cuenta pueda iniciar sesión de inmediato (RF-CA-19, RF-CA-20).
        usuario.IntentosFallidosConsecutivos = 0;
        usuario.BloqueadoHasta = null;

        await unidad.GuardarCambiosAsync(cancellationToken);
    }

    private async Task<Usuario> ObtenerRequeridoAsync(int usuarioId, CancellationToken cancellationToken)
        => await unidad.Usuarios.BuscarPorIdAsync(usuarioId, cancellationToken)
           ?? throw new ReglaNegocioExcepcion("El usuario no existe.", 404);
}