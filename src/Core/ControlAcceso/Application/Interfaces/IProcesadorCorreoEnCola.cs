using Core.ControlAcceso.Application.Modelos;

namespace Core.ControlAcceso.Application.Interfaces;

public interface IProcesadorCorreoEnCola
{
    /// <summary>
    /// Toma los correos en estado Pendiente, los envía con <see cref="IEnviadorCorreo"/>
    /// y los marca Enviado o registra el fallo (RF-NOT-08, RF-NOT-10). Idempotente y
    /// seguro frente a ejecuciones concurrentes (RF-NOT-09, RF-NOT-12). Devuelve la
    /// cantidad de correos enviados y fallidos en la pasada.
    /// </summary>
    Task<ResultadoProcesamientoCorreos> ProcesarAsync(CancellationToken cancellationToken = default);
}