using Core.Auditoria.Application.Interfaces;
using Core.ControlAcceso.Application.Interfaces;
using Core.ControlAcceso.Application.Servicios;
using Core.ControlAcceso.Domain.Entidades;
using Core.ControlAcceso.Domain.Enums;
using Core.Documentos.Application.Interfaces;
using Core.Notificaciones.Application.Interfaces;
using Core.Permisos.Application.Interfaces;
using Tests.Core.Dobles;
using Xunit;

namespace Tests.Core;

/// <summary>
/// C-15, C-16 (RF-NOT-10): reintentos, fallo controlado y continuación de la pasada.
/// </summary>
public class RF_NOT_10_ReintentosProcesadorCorreoTests
{
    [Fact]
    public async Task RF_NOT_10_TresFallos_MarcaFallidoConIntentosErrorYFecha_YNoSeVuelveAIntentar()
    {
        // C-15: tres fallos → Fallido con intentos 3, último error y fecha; no se reintenta.
        // Preparar
        var fechaIntento = new DateTimeOffset(2026, 11, 18, 10, 0, 0, TimeSpan.Zero);
        var reloj = new TimeProviderFijo(fechaIntento);
        var correo = CrearCorreoPendiente(1, intentos: 0);
        var repo = new CorreosEnColaRepositorioFalso([correo]);
        var enviador = new EnviadorCorreoFalso(fallaSiempre: true);
        var procesador = new ProcesadorCorreoEnCola(new UnidadTrabajoFalsa(repo), enviador, reloj);

        // Actuar
        var primera = await procesador.ProcesarAsync(CancellationToken.None);
        var segunda = await procesador.ProcesarAsync(CancellationToken.None);
        var tercera = await procesador.ProcesarAsync(CancellationToken.None);
        var cuarta = await procesador.ProcesarAsync(CancellationToken.None);

        // Verificar
        Assert.Equal(0, primera.Enviados);
        Assert.Equal(1, primera.Fallidos);
        Assert.Equal(1, segunda.Fallidos);
        Assert.Equal(1, tercera.Fallidos);
        Assert.Equal(EstadoCorreo.Fallido, correo.Estado);
        Assert.Equal(3, correo.Intentos);
        Assert.Equal("InvalidOperationException: boom", correo.UltimoError);
        Assert.Equal(fechaIntento.UtcDateTime, correo.FechaUltimoIntento);
        Assert.Equal(0, cuarta.Enviados);
        Assert.Equal(0, cuarta.Fallidos);
        Assert.Equal(3, enviador.Llamadas);
    }

    [Fact]
    public async Task RF_NOT_10_UnoODosFallos_QuedaPendienteConIntentosContados()
    {
        // C-15: uno o dos fallos → Pendiente con sus intentos contados.
        // Preparar
        var fechaIntento = new DateTimeOffset(2026, 11, 18, 11, 0, 0, TimeSpan.Zero);
        var reloj = new TimeProviderFijo(fechaIntento);
        var correo = CrearCorreoPendiente(1, intentos: 0);
        var repo = new CorreosEnColaRepositorioFalso([correo]);
        var enviador = new EnviadorCorreoFalso(fallaSiempre: true);
        var procesador = new ProcesadorCorreoEnCola(new UnidadTrabajoFalsa(repo), enviador, reloj);

        // Actuar
        var primera = await procesador.ProcesarAsync(CancellationToken.None);
        var estadoTrasUno = correo.Estado;
        var intentosTrasUno = correo.Intentos;

        var segunda = await procesador.ProcesarAsync(CancellationToken.None);

        // Verificar
        Assert.Equal(1, primera.Fallidos);
        Assert.Equal(EstadoCorreo.Pendiente, estadoTrasUno);
        Assert.Equal(1, intentosTrasUno);
        Assert.Equal(1, segunda.Fallidos);
        Assert.Equal(EstadoCorreo.Pendiente, correo.Estado);
        Assert.Equal(2, correo.Intentos);
        Assert.Equal(fechaIntento.UtcDateTime, correo.FechaUltimoIntento);
        Assert.Equal("InvalidOperationException: boom", correo.UltimoError);
    }

    [Fact]
    public async Task RF_NOT_10_TresPendientesConFalloEnElSegundo_ContinuaLaPasada()
    {
        // C-16: tres pendientes, el segundo falla: primero y tercero Enviado, segundo Pendiente.
        // Preparar
        var fechaIntento = new DateTimeOffset(2026, 11, 18, 12, 0, 0, TimeSpan.Zero);
        var reloj = new TimeProviderFijo(fechaIntento);
        var primero = CrearCorreoPendiente(1, intentos: 0);
        var segundo = CrearCorreoPendiente(2, intentos: 0);
        var tercero = CrearCorreoPendiente(3, intentos: 0);
        var repo = new CorreosEnColaRepositorioFalso([primero, segundo, tercero]);
        var enviador = new EnviadorCorreoFalso(idsQueFallan: [2]);
        var procesador = new ProcesadorCorreoEnCola(new UnidadTrabajoFalsa(repo), enviador, reloj);

        // Actuar
        var resultado = await procesador.ProcesarAsync(CancellationToken.None);

        // Verificar
        Assert.Equal(2, resultado.Enviados);
        Assert.Equal(1, resultado.Fallidos);
        Assert.Equal(EstadoCorreo.Enviado, primero.Estado);
        Assert.Equal(1, primero.Intentos);
        Assert.Equal(fechaIntento.UtcDateTime, primero.FechaEnvio);
        Assert.Equal(fechaIntento.UtcDateTime, primero.FechaUltimoIntento);
        Assert.Equal(EstadoCorreo.Pendiente, segundo.Estado);
        Assert.Equal(1, segundo.Intentos);
        Assert.Equal("InvalidOperationException: boom", segundo.UltimoError);
        Assert.Null(segundo.FechaEnvio);
        Assert.Equal(EstadoCorreo.Enviado, tercero.Estado);
        Assert.Equal(1, tercero.Intentos);
        Assert.Equal([1, 2, 3], enviador.IdsIntentados);
    }

    private static CorreoEnCola CrearCorreoPendiente(int id, int intentos)
        => new()
        {
            Id = id,
            Destinatario = $"destino{id}@ejemplo.com",
            Asunto = $"Asunto {id}",
            Cuerpo = $"Cuerpo {id}",
            Estado = EstadoCorreo.Pendiente,
            Intentos = intentos,
            FechaCreacion = new DateTime(2026, 11, 18, 8, 0, 0, DateTimeKind.Utc),
        };

    private sealed class UnidadTrabajoFalsa(ICorreosEnColaRepositorio correos) : ICoreUnidadDeTrabajo
    {
        public ICorreosEnColaRepositorio CorreosEnCola { get; } = correos;

        public IUsuariosRepositorio Usuarios => null!;
        public ITokenActivacionesRepositorio TokenActivaciones => null!;
        public ICodigosRecuperacionRepositorio CodigosRecuperacion => null!;
        public ISolicitudesPermisoRepositorio SolicitudesPermiso => null!;
        public IPermisosActivosRepositorio PermisosActivos => null!;
        public IDocumentosRepositorio Documentos => null!;
        public INotificacionesRepositorio Notificaciones => null!;
        public IPreferenciasNotificacionRepositorio PreferenciasNotificacion => null!;
        public IRegistrosAuditoriaRepositorio RegistrosAuditoria => null!;

        public Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(0);

        public void Dispose()
        {
        }
    }

    private sealed class CorreosEnColaRepositorioFalso(List<CorreoEnCola> correos) : ICorreosEnColaRepositorio
    {
        public Task AgregarAsync(CorreoEnCola correo, CancellationToken cancellationToken = default)
        {
            correos.Add(correo);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<CorreoEnCola>> ObtenerPendientesAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<CorreoEnCola>>(
                correos.Where(c => c.Estado == EstadoCorreo.Pendiente).OrderBy(c => c.Id).ToList());

        public Task<bool> ReclamarAsync(int id, CancellationToken cancellationToken = default)
        {
            var correo = correos.Single(c => c.Id == id);
            if (correo.Estado != EstadoCorreo.Pendiente)
            {
                return Task.FromResult(false);
            }

            correo.Estado = EstadoCorreo.Enviando;
            return Task.FromResult(true);
        }

        public Task MarcarEnviadoAsync(
            int id,
            DateTime fechaEnvioUtc,
            int intentos,
            DateTime fechaUltimoIntentoUtc,
            CancellationToken cancellationToken = default)
        {
            var correo = correos.Single(c => c.Id == id);
            if (correo.Estado != EstadoCorreo.Enviando)
            {
                return Task.CompletedTask;
            }

            correo.Estado = EstadoCorreo.Enviado;
            correo.FechaEnvio = fechaEnvioUtc;
            correo.Intentos = intentos;
            correo.FechaUltimoIntento = fechaUltimoIntentoUtc;
            return Task.CompletedTask;
        }

        public Task RegistrarFalloAsync(
            int id,
            int intentos,
            DateTime fechaUltimoIntentoUtc,
            string ultimoError,
            bool quedaFallido,
            CancellationToken cancellationToken = default)
        {
            var correo = correos.Single(c => c.Id == id);
            if (correo.Estado != EstadoCorreo.Enviando)
            {
                return Task.CompletedTask;
            }

            correo.Intentos = intentos;
            correo.FechaUltimoIntento = fechaUltimoIntentoUtc;
            correo.UltimoError = ultimoError;
            correo.Estado = quedaFallido ? EstadoCorreo.Fallido : EstadoCorreo.Pendiente;
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<CorreoEnCola>> ListarAsync(
            EstadoCorreo? estado,
            CancellationToken cancellationToken = default)
        {
            var consulta = estado is null
                ? correos.AsEnumerable()
                : correos.Where(c => c.Estado == estado);

            return Task.FromResult<IReadOnlyList<CorreoEnCola>>(
                consulta.OrderByDescending(c => c.FechaCreacion).ThenByDescending(c => c.Id).ToList());
        }
    }

    private sealed class EnviadorCorreoFalso(bool fallaSiempre = false, IReadOnlyCollection<int>? idsQueFallan = null)
        : IEnviadorCorreo
    {
        private readonly HashSet<int> _idsQueFallan = idsQueFallan is null ? [] : [.. idsQueFallan];

        public int Llamadas { get; private set; }

        public List<int> IdsIntentados { get; } = [];

        public Task EnviarAsync(string destinatario, string asunto, string cuerpo, CancellationToken cancellationToken = default)
        {
            Llamadas++;
            var id = int.Parse(destinatario.Replace("destino", string.Empty).Replace("@ejemplo.com", string.Empty));
            IdsIntentados.Add(id);

            if (fallaSiempre || _idsQueFallan.Contains(id))
            {
                throw new InvalidOperationException("boom");
            }

            return Task.CompletedTask;
        }
    }
}
