using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Tams.Negocio.Infrastructura.Persistence;

public static class TamsNegocioPersistenceExtensions
{
    /// <summary>
    /// Registra el <see cref="TamsDbContext"/> del módulo de negocio.
    /// La cadena de conexión se recibe desde configuración/variables de entorno (RD-10);
    /// quien invoque esta extensión (normalmente el host) es responsable de proveerla.
    /// Además registra un <see cref="TimeProvider"/> por defecto para sellar
    /// las fechas de creación en UTC (RD-11, RD-12).
    /// </summary>
    public static IServiceCollection AddTamsNegocioDbContext(
        this IServiceCollection services,
        string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.TryAddSingleton(TimeProvider.System);

        return services.AddDbContext<TamsDbContext>(options => options.UseSqlServer(connectionString));
    }
}