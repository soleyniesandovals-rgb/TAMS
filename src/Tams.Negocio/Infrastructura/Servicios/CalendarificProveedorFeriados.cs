using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Tams.Negocio.Application.Excepciones;
using Tams.Negocio.Application.Servicios;

namespace Tams.Negocio.Infrastructura.Servicios;

/// <summary>
/// Proveedor de feriados nacionales de Calendarific (RD-03, RD-10).
/// La clave se lee en cada llamada para que la API pueda arrancar sin ella; nunca se
/// registra la URL porque contiene la clave en la query.
/// </summary>
public class CalendarificProveedorFeriados(
    HttpClient cliente,
    ILogger<CalendarificProveedorFeriados> logger) : IProveedorFeriados
{
    internal const string NombreVariableClave = "TAMS_API_FERIADOS_CLAVE";

    private const string Pais = "DO";
    private const int CodigoOperacionExitosa = 200;

    public async Task<IReadOnlyList<(DateOnly Fecha, string Nombre)>> ObtenerFeriadosNacionalesAsync(
        int anio,
        CancellationToken cancellationToken = default)
    {
        var clave = Environment.GetEnvironmentVariable(NombreVariableClave);

        if (string.IsNullOrWhiteSpace(clave))
        {
            logger.LogWarning("Feriados: falta la configuración del proveedor.");
            throw new ServicioFeriadosNoConfiguradoExcepcion();
        }

        // La clave viaja en la query; esta URL nunca se registra ni se incluye en una
        // excepción o en un mensaje de error.
        var ruta = $"api/v2/holidays?api_key={Uri.EscapeDataString(clave)}"
                   + $"&country={Pais}&year={anio}&type=national";

        HttpResponseMessage respuesta;

        try
        {
            respuesta = await cliente.GetAsync(ruta, cancellationToken);
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("Feriados: tiempo de espera agotado ({TipoError}).", ex.GetType().Name);
            throw new ServicioFeriadosNoDisponibleExcepcion(ex);
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning("Feriados: error de red ({TipoError}).", ex.GetType().Name);
            throw new ServicioFeriadosNoDisponibleExcepcion(ex);
        }

        using (respuesta)
        {
            if (!respuesta.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "Feriados: Calendarific respondió HTTP {CodigoHttp}.",
                    (int)respuesta.StatusCode);
                throw new ServicioFeriadosNoDisponibleExcepcion();
            }

            RespuestaCalendarific? contenido;

            try
            {
                contenido = await respuesta.Content.ReadFromJsonAsync<RespuestaCalendarific>(cancellationToken);
            }
            catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                logger.LogWarning("Feriados: tiempo de espera agotado ({TipoError}).", ex.GetType().Name);
                throw new ServicioFeriadosNoDisponibleExcepcion(ex);
            }
            catch (Exception ex) when (ex is JsonException or NotSupportedException or IOException or HttpRequestException)
            {
                logger.LogWarning("Feriados: respuesta inválida ({TipoError}).", ex.GetType().Name);
                throw new ServicioFeriadosNoDisponibleExcepcion(ex);
            }

            if (contenido?.Meta?.Code != CodigoOperacionExitosa)
            {
                logger.LogWarning("Feriados: Calendarific devolvió un código de operación no exitoso.");
                throw new ServicioFeriadosNoDisponibleExcepcion();
            }

            if (contenido.Response is null)
            {
                logger.LogWarning("Feriados: la respuesta no contiene el objeto response.");
                throw new ServicioFeriadosNoDisponibleExcepcion();
            }

            var feriados = new List<(DateOnly Fecha, string Nombre)>();

            foreach (var feriado in contenido.Response.Holidays ?? [])
            {
                if (!TryParseFecha(feriado.Date?.Iso, out var fecha))
                {
                    logger.LogWarning("Feriados: la respuesta contiene una fecha inválida.");
                    throw new ServicioFeriadosNoDisponibleExcepcion();
                }

                feriados.Add((fecha, feriado.Name ?? string.Empty));
            }

            return feriados;
        }
    }

    private static bool TryParseFecha(string? iso, out DateOnly fecha)
    {
        fecha = default;

        if (string.IsNullOrWhiteSpace(iso))
        {
            return false;
        }

        // La API puede devolver "2026-01-01" o "2026-01-01T00:00:00.000Z".
        var parteFecha = iso.Length >= 10 ? iso[..10] : iso;

        return DateOnly.TryParseExact(
            parteFecha,
            "yyyy-MM-dd",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out fecha);
    }

    private sealed record RespuestaCalendarific(
        CalendarificMeta? Meta,
        CalendarificRespuesta? Response);

    private sealed record CalendarificMeta(int? Code);

    private sealed record CalendarificRespuesta(
        List<CalendarificFeriado>? Holidays);

    private sealed record CalendarificFeriado(
        string? Name,
        CalendarificFecha? Date);

    private sealed record CalendarificFecha(string? Iso);
}
