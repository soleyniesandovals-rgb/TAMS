namespace Core.ControlAcceso.Application.Interfaces;

public interface IProcesadorCorreoEnCola
{
    /// <summary>
    /// Toma los correos en estado Pendiente, los envía con <see cref="IEnviadorCorreo"/>
    /// y los marca Enviado (RF-NOT-08). Idempotente y seguro frente a ejecuciones
    /// concurrentes (RF-NOT-09). Devuelve la cantidad de correos enviados.
    /// </summary>
    Task<int> ProcesarAsync(CancellationToken cancellationToken = default);
}