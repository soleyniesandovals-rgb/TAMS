using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Core.ControlAcceso.Infrastructura.Persistence;

public static class CorePersistenceExtensions
{
    /// <summary>
    /// Registra el <see cref="CoreDbContext"/> del Core.
    /// La cadena de conexión se recibe desde configuración/variables de entorno (RD-10);
    /// quien invoque esta extensión (normalmente el host) es responsable de proveerla.
    /// Además registra un <see cref="TimeProvider"/> por defecto para sellar
    /// las fechas de creación en UTC (RD-11, RD-12).
    /// RD-03: el Core usa el esquema "core" y su propia tabla de historial de migraciones.
    /// </summary>
    public static IServiceCollection AddCoreDbContext(
        this IServiceCollection services,
        string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.TryAddSingleton(TimeProvider.System);

        return services.AddDbContext<CoreDbContext>(options =>
            options.UseSqlServer(connectionString,
                sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "core")));
    }
}