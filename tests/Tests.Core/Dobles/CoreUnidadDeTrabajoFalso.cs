using Core.Auditoria.Application.Interfaces;
using Core.Auditoria.Application.Modelos;
using Core.Auditoria.Domain.Entidades;
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
    private readonly List<int> _registrosPorGuardado = [];

    public CoreUnidadDeTrabajoFalso(TimeProvider reloj)
    {
        Usuarios = new UsuariosFalso();
        CorreosEnCola = new CorreosFalso(reloj, CorreosGuardados);
        CodigosRecuperacion = new CodigosFalso(CodigosGuardados);
        Notificaciones = new NotificacionesFalso(reloj, NotificacionesGuardadas);
        PreferenciasNotificacion = new PreferenciasFalso(PreferenciasGuardadas);
        RegistrosAuditoria = new RegistrosAuditoriaFalso(Registros);
    }

    /// <summary>Registros de auditoría agregados por el registrador (CA-01).</summary>
    public List<RegistroAuditoria> Registros { get; } = [];

    /// <summary>Cantidad de registros presentes en cada guardado (CA-12).</summary>
    public IReadOnlyList<int> RegistrosPorGuardado => _registrosPorGuardado;

    public int Guardados { get; private set; }

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

    public IRegistrosAuditoriaRepositorio RegistrosAuditoria { get; }

    public Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default)
    {
        Guardados++;
        _registrosPorGuardado.Add(Registros.Count);
        return Task.FromResult(0);
    }

    public Task<T> EjecutarEnTransaccionAsync<T>(
        Func<CancellationToken, Task<T>> operacion,
        CancellationToken cancellationToken = default)
        => operacion(cancellationToken);

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
            => Task.FromResult<IReadOnlyList<CorreoEnCola>>(
                correos.Where(c => c.Estado == EstadoCorreo.Pendiente).ToList());

        public Task<bool> ReclamarAsync(int id, CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task MarcarEnviadoAsync(
            int id,
            DateTime fechaEnvioUtc,
            int intentos,
            DateTime fechaUltimoIntentoUtc,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task RegistrarFalloAsync(
            int id,
            int intentos,
            DateTime fechaUltimoIntentoUtc,
            string ultimoError,
            bool quedaFallido,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<IReadOnlyList<CorreoEnCola>> ListarAsync(
            EstadoCorreo? estado,
            CancellationToken cancellationToken = default)
        {
            var consulta = estado is null
                ? correos.AsEnumerable()
                : correos.Where(c => c.Estado == estado);

            return Task.FromResult<IReadOnlyList<CorreoEnCola>>(consulta.ToList());
        }
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

    private sealed class RegistrosAuditoriaFalso(List<RegistroAuditoria> registros) : IRegistrosAuditoriaRepositorio
    {
        public Task AgregarAsync(RegistroAuditoria registro, CancellationToken cancellationToken = default)
        {
            registros.Add(registro);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<RegistroAuditoria>> ConsultarAsync(
            FiltrosConsultaAuditoria filtros,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException("Este doble solo registra; la consulta usa EF Core InMemory (RD-12).");
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
