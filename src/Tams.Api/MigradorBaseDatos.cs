using Core.ControlAcceso.Infrastructura.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tams.Negocio.Infrastructura.Persistence;

namespace Tams.Api;

/// <summary>
/// Aplica las migraciones de los dos DbContext al arrancar la API (RD-09).
/// Reintenta para dar tiempo a que SQL Server termine de iniciar dentro del
/// contenedor. Nunca registra la cadena de conexión.
/// </summary>
public static class MigradorBaseDatos
{
    private const int MaxIntentos = 30;
    private static readonly TimeSpan EsperaEntreIntentos = TimeSpan.FromSeconds(2);

    public static async Task AplicarAsync(
        IServiceProvider servicios,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        var intento = 0;

        while (true)
        {
            intento++;

            try
            {
                using var scope = servicios.CreateScope();

                logger.LogInformation(
                    "Intento {Intento}/{Maximo}: aplicando migraciones de Core...",
                    intento,
                    MaxIntentos);

                var core = scope.ServiceProvider.GetRequiredService<CoreDbContext>();
                await core.Database.MigrateAsync(cancellationToken);

                logger.LogInformation(
                    "Intento {Intento}/{Maximo}: aplicando migraciones de Negocio...",
                    intento,
                    MaxIntentos);

                var negocio = scope.ServiceProvider.GetRequiredService<TamsDbContext>();
                await negocio.Database.MigrateAsync(cancellationToken);

                logger.LogInformation(
                    "Migraciones de Core y Negocio aplicadas correctamente en el intento {Intento}.",
                    intento);

                return;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                if (intento >= MaxIntentos)
                {
                    logger.LogCritical(
                        "No se pudieron aplicar las migraciones después de {Maximo} intentos. Último error: {Tipo}.",
                        MaxIntentos,
                        ex.GetType().Name);

                    throw new InvalidOperationException(
                        $"No se pudieron aplicar las migraciones de la base de datos después de {MaxIntentos} intentos. "
                        + "Verifique que SQL Server esté disponible y que las variables "
                        + "TAMS_CORE_CONNECTION_STRING y TAMS_NEGOCIO_CONNECTION_STRING sean correctas.",
                        ex);
                }

                logger.LogWarning(
                    "Intento {Intento}/{Maximo} falló al aplicar migraciones ({Tipo}). "
                    + "Reintentando en {Segundos} s.",
                    intento,
                    MaxIntentos,
                    ex.GetType().Name,
                    EsperaEntreIntentos.TotalSeconds);

                await Task.Delay(EsperaEntreIntentos, cancellationToken);
            }
        }
    }
}
