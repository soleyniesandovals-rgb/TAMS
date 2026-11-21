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
/// C-18 (RF-NOT-12): idempotencia del procesador ante ejecuciones repetidas.
/// </summary>
public class RF_NOT_12_IdempotenciaProcesadorCorreoTests
{
    [Fact]
    public async Task RF_NOT_12_DosPasadasSeguidas_NoEnvianDosVecesElMismoCorreo()
    {
        // C-18: dos pasadas seguidas no envían dos veces el mismo correo; cuenta llamadas.
        // Preparar
        var reloj = new TimeProviderFijo(new DateTimeOffset(2026, 11, 18, 13, 0, 0, TimeSpan.Zero));
        var correo = new CorreoEnCola
        {
            Id = 1,
            Destinatario = "destino@ejemplo.com",
            Asunto = "Asunto",
            Cuerpo = "Cuerpo",
            Estado = EstadoCorreo.Pendiente,
            Intentos = 0,
            FechaCreacion = new DateTime(2026, 11, 18, 8, 0, 0, DateTimeKind.Utc),
        };
        var repo = new CorreosEnColaRepositorioFalso([correo]);
        var enviador = new EnviadorCorreoFalso();
        var procesador = new ProcesadorCorreoEnCola(new UnidadTrabajoFalsa(repo), enviador, reloj);

        // Actuar
        var primera = await procesador.ProcesarAsync(CancellationToken.None);
        var segunda = await procesador.ProcesarAsync(CancellationToken.None);

        // Verificar
        Assert.Equal(1, primera.Enviados);
        Assert.Equal(0, primera.Fallidos);
        Assert.Equal(0, segunda.Enviados);
        Assert.Equal(0, segunda.Fallidos);
        Assert.Equal(1, enviador.Llamadas);
        Assert.Equal(EstadoCorreo.Enviado, correo.Estado);
    }

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

        public Task<T> EjecutarEnTransaccionAsync<T>(
            Func<CancellationToken, Task<T>> operacion,
            CancellationToken cancellationToken = default)
            => operacion(cancellationToken);

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

    private sealed class EnviadorCorreoFalso : IEnviadorCorreo
    {
        public int Llamadas { get; private set; }

        public Task EnviarAsync(string destinatario, string asunto, string cuerpo, CancellationToken cancellationToken = default)
        {
            Llamadas++;
            return Task.CompletedTask;
        }
    }
}
