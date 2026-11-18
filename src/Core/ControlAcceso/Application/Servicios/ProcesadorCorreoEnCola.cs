using Core.ControlAcceso.Application.Interfaces;
using Core.ControlAcceso.Application.Modelos;
using Core.ControlAcceso.Application.Reglas;

namespace Core.ControlAcceso.Application.Servicios;

public class ProcesadorCorreoEnCola(
    ICoreUnidadDeTrabajo unidad,
    IEnviadorCorreo enviador,
    TimeProvider timeProvider) : IProcesadorCorreoEnCola
{
    public async Task<ResultadoProcesamientoCorreos> ProcesarAsync(CancellationToken cancellationToken = default)
    {
        var pendientes = await unidad.CorreosEnCola.ObtenerPendientesAsync(cancellationToken);

        var enviados = 0;
        var fallidos = 0;

        foreach (var correo in pendientes)
        {
            // RF-NOT-09, C-18: reclamo condicionado al estado actual. Si otra ejecución
            // ya tomó este correo, el UPDATE afecta 0 filas y este proceso lo salta.
            var reclamado = await unidad.CorreosEnCola.ReclamarAsync(correo.Id, cancellationToken);
            if (!reclamado)
            {
                continue;
            }

            var fechaIntento = timeProvider.GetUtcNow().UtcDateTime;
            var intentos = correo.Intentos + 1;

            try
            {
                await enviador.EnviarAsync(correo.Destinatario, correo.Asunto, correo.Cuerpo, cancellationToken);
                await unidad.CorreosEnCola.MarcarEnviadoAsync(
                    correo.Id, fechaIntento, intentos, fechaIntento, cancellationToken);
                enviados++;
            }
            catch (Exception ex)
            {
                // RF-NOT-10, C-15, C-16: se registra el fallo y se continúa con el siguiente;
                // un fallo no corta la pasada. Si se agotaron los intentos, el correo queda
                // Fallido y no se vuelve a tomar.
                var ultimoError = FormatearError(ex);
                var quedaFallido = intentos >= ReglasColaCorreos.MaximoIntentos;
                await unidad.CorreosEnCola.RegistrarFalloAsync(
                    correo.Id, intentos, fechaIntento, ultimoError, quedaFallido, cancellationToken);
                fallidos++;
            }
        }

        return new ResultadoProcesamientoCorreos(enviados, fallidos);
    }

    private static string FormatearError(Exception ex)
    {
        var mensaje = $"{ex.GetType().Name}: {ex.Message}";

        return mensaje.Length <= 500 ? mensaje : mensaje[..500];
    }
}