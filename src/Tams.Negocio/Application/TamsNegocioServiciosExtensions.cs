using Microsoft.Extensions.DependencyInjection;
using Tams.Negocio.Application.Servicios;

namespace Tams.Negocio.Application;

/// <summary>
/// Punto de entrada único para registrar los servicios de aplicación del módulo de negocio
/// (RD-12). El host decide cuándo invocarlo; este bloque no lo conecta desde Program.cs.
/// </summary>
public static class TamsNegocioServiciosExtensions
{
    /// <summary>Registra los servicios de aplicación del módulo como Scoped.</summary>
    public static IServiceCollection AddTamsNegocioServicios(this IServiceCollection services)
    {
        services.AddScoped<ICalificacionesServicio, CalificacionesServicio>();

        return services;
    }
}
