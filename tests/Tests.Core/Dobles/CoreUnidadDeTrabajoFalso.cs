using Core.ControlAcceso.Application.Interfaces;
using Core.ControlAcceso.Domain.Entidades;
using Core.ControlAcceso.Domain.Enums;
using Core.Documentos.Application.Interfaces;
using Core.Notificaciones.Application.Interfaces;
using Core.Notificaciones.Domain.Entidades;
using Core.Permisos.Application.Interfaces;

namespace Tests.Core.Dobles;

/// <summary>
/// Unidad de trabajo en memoria para C-11: evita los ExecuteUpdate que InMemory no soporta
/// y permite comprobar que la preferencia no afecta a los correos de cuenta (RD-12).
/// </summary>
public sealed class CoreUnidadDeTrabajoFalso : ICoreUnidadDeTrabajo
{
    public CoreUnidadDeTrabajoFalso(TimeProvider reloj)
    {
        Usuarios = new UsuariosFalso();
        CorreosEnCola = new CorreosFalso(reloj, CorreosGuardados);
        CodigosRecuperacion = new CodigosFalso(CodigosGuardados);
        Notificaciones = new NotificacionesFalso(reloj, NotificacionesGuardadas);
        PreferenciasNotificacion = new PreferenciasFalso(PreferenciasGuardadas);
    }

    public List<CorreoEnCola> CorreosGuardados { get; } = [];

    public List<CodigoRecuperacion> CodigosGuardados { get; } = [];

    public List<Notificacion> NotificacionesGuardadas { get; } = [];

    public List<PreferenciaNotificacion> PreferenciasGuardadas { get; } = [];

    public IUsuariosRepositorio Usuarios { get; }

    public ITokenActivacionesRepositorio TokenActivaciones => null!;

    public ICorreosEnColaRepositorio CorreosEnCola { get; }

    public ICodigosRecuperacionRepositorio CodigosRecuperacion { get; }

    public ISolicitudesPermisoRepositorio SolicitudesPermiso => null!;

    public IPermisosActivosRepositorio PermisosActivos => null!;

    public IDocumentosRepositorio Documentos => null!;

    public INotificacionesRepositorio Notificaciones { get; }

    public IPreferenciasNotificacionRepositorio PreferenciasNotificacion { get; }

    public Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(0);

    public void Dispose()
    {
    }

    private sealed class UsuariosFalso : IUsuariosRepositorio
    {
        private readonly List<Usuario> _usuarios = [];

        public Task<Usuario?> BuscarPorCorreoAsync(string correo, CancellationToken cancellationToken = default)
            => Task.FromResult(_usuarios.FirstOrDefault(u => u.Correo == correo));

        public Task<Usuario?> BuscarPorIdAsync(int id, CancellationToken cancellationToken = default)
            => Task.FromResult(_usuarios.FirstOrDefault(u => u.Id == id));

        public Task<IReadOnlyList<Usuario>> BuscarPorIdsAsync(
            IReadOnlyCollection<int> ids,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Usuario>>(_usuarios.Where(u => ids.Contains(u.Id)).ToList());

        public Task<IReadOnlyList<Usuario>> ListarAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Usuario>>(_usuarios.ToList());

        public Task<IReadOnlyList<Usuario>> ListarAdministradoresActivosAsync(
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Usuario>>(
                _usuarios.Where(u => u.Rol == RolUsuario.Administrador && u.Activo).ToList());

        public Task AgregarAsync(Usuario usuario, CancellationToken cancellationToken = default)
        {
            _usuarios.Add(usuario);
            return Task.CompletedTask;
        }
    }

    private sealed class CorreosFalso(TimeProvider reloj, List<CorreoEnCola> correos) : ICorreosEnColaRepositorio
    {
        public Task AgregarAsync(CorreoEnCola correo, CancellationToken cancellationToken = default)
        {
            correo.Id = correos.Count + 1;
            correo.FechaCreacion = reloj.GetUtcNow().UtcDateTime;
            correos.Add(correo);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<CorreoEnCola>> ObtenerPendientesAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<CorreoEnCola>>(correos.ToList());

        public Task<bool> ReclamarAsync(int id, CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task MarcarEnviadoAsync(int id, DateTime fechaEnvioUtc, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task ReactivarAsync(int id, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class CodigosFalso(List<CodigoRecuperacion> codigos) : ICodigosRecuperacionRepositorio
    {
        public Task<CodigoRecuperacion?> BuscarPorCodigoAsync(string codigo, CancellationToken cancellationToken = default)
            => Task.FromResult(codigos.FirstOrDefault(c => c.Codigo == codigo));

        public Task AgregarAsync(CodigoRecuperacion codigo, CancellationToken cancellationToken = default)
        {
            codigo.Id = codigos.Count + 1;
            codigos.Add(codigo);
            return Task.CompletedTask;
        }

        public Task<int> MarcarUsadosDelUsuarioAsync(int usuarioId, CancellationToken cancellationToken = default)
        {
            foreach (var codigo in codigos.Where(c => c.UsuarioId == usuarioId))
            {
                codigo.Usado = true;
            }

            return Task.FromResult(0);
        }
    }

    private sealed class NotificacionesFalso(
        TimeProvider reloj,
        List<Notificacion> notificaciones) : INotificacionesRepositorio
    {
        public Task AgregarAsync(Notificacion notificacion, CancellationToken cancellationToken = default)
        {
            notificacion.Id = notificaciones.Count + 1;
            notificacion.FechaCreacion = reloj.GetUtcNow().UtcDateTime;
            notificaciones.Add(notificacion);
            return Task.CompletedTask;
        }

        public Task<Notificacion?> BuscarPorIdAsync(int id, CancellationToken cancellationToken = default)
            => Task.FromResult(notificaciones.FirstOrDefault(n => n.Id == id));

        public Task<IReadOnlyList<Notificacion>> ListarPorUsuarioAsync(
            int usuarioId,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Notificacion>>(notificaciones.ToList());
    }

    private sealed class PreferenciasFalso(List<PreferenciaNotificacion> preferencias)
        : IPreferenciasNotificacionRepositorio
    {
        public Task<PreferenciaNotificacion?> ObtenerAsync(int usuarioId, CancellationToken cancellationToken = default)
            => Task.FromResult(preferencias.FirstOrDefault(p => p.UsuarioId == usuarioId));

        public Task AgregarAsync(PreferenciaNotificacion preferencia, CancellationToken cancellationToken = default)
        {
            preferencias.Add(preferencia);
            return Task.CompletedTask;
        }
    }
}
