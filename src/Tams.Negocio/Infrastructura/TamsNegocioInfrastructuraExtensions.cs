using Microsoft.Extensions.DependencyInjection;
using Tams.Negocio.Application.Servicios;
using Tams.Negocio.Infrastructura.Servicios;

namespace Tams.Negocio.Infrastructura;

/// <summary>
/// Registro de las implementaciones de infraestructura del módulo de negocio.
/// </summary>
public static class TamsNegocioInfrastructuraExtensions
{
    /// <summary>
    /// Registra el proveedor de feriados con un HttpClient tipado (RD-03, RD-10).
    /// La clave no se lee al arrancar: el proveedor la busca en cada llamada.
    /// </summary>
    public static IServiceCollection AddTamsNegocioInfrastructura(this IServiceCollection services)
    {
        services.AddHttpClient<IProveedorFeriados, CalendarificProveedorFeriados>(cliente =>
            {
                cliente.BaseAddress = new Uri("https://calendarific.com/");
                cliente.Timeout = TimeSpan.FromSeconds(10);
            })
            // El logging por defecto de IHttpClientFactory registra la URI de cada
            // petición. En .NET 9+ la query se redacta, pero se desactiva para este
            // cliente para que ni siquiera aparezca la URL que contiene la clave.
            .RemoveAllLoggers();

        return services;
    }
}
