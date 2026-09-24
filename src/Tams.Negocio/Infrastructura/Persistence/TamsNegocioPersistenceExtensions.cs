using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Tams.Negocio.Infrastructura.Persistence;

public static class TamsNegocioPersistenceExtensions
{
    /// <summary>
    /// Registra el <see cref="TamsDbContext"/> del módulo de negocio.
    /// La cadena de conexión se recibe desde configuración/variables de entorno (RD-10);
    /// quien invoque esta extensión (normalmente el host) es responsable de proveerla.
    /// </summary>
    public static IServiceCollection AddTamsNegocioDbContext(
        this IServiceCollection services,
        string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        return services.AddDbContext<TamsDbContext>(options => options.UseSqlServer(connectionString));
    }
}