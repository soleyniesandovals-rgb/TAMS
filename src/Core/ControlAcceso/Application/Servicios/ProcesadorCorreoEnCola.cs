using Core.ControlAcceso.Application.Interfaces;

namespace Core.ControlAcceso.Application.Servicios;

public class ProcesadorCorreoEnCola(
    ICoreUnidadDeTrabajo unidad,
    IEnviadorCorreo enviador,
    TimeProvider timeProvider) : IProcesadorCorreoEnCola
{
    public async Task<int> ProcesarAsync(CancellationToken cancellationToken = default)
    {
        var pendientes = await unidad.CorreosEnCola.ObtenerPendientesAsync(cancellationToken);

        var enviados = 0;
        foreach (var correo in pendientes)
        {
            // RF-NOT-09: reclamo condicionado al estado actual. Si otra ejecución ya
            // tomó este correo (Estado ya no es Pendiente), el UPDATE afecta 0 filas y
            // este proceso lo salta: nunca se envía dos veces el mismo correo.
            var reclamado = await unidad.CorreosEnCola.ReclamarAsync(correo.Id, cancellationToken);
            if (!reclamado)
            {
                continue;
            }

            try
            {
                await enviador.EnviarAsync(correo.Destinatario, correo.Asunto, correo.Cuerpo, cancellationToken);
                await unidad.CorreosEnCola.MarcarEnviadoAsync(correo.Id, timeProvider.GetUtcNow().UtcDateTime, cancellationToken);
                enviados++;
            }
            catch
            {
                // SMTP caído o error transitorio: se devuelve el correo a Pendiente para
                // reintentarlo en la próxima ejecución. La operación que lo originó
                // (registro/reenvío) ya terminó y se confirmó antes de llegar aquí.
                try
                {
                    await unidad.CorreosEnCola.ReactivarAsync(correo.Id, cancellationToken);
                }
                catch
                {
                    // Si tampoco se puede reactivar, el correo permanece Enviando y
                    // podrá retomarse en una intervención posterior.
                }

                throw;
            }
        }

        return enviados;
    }
}