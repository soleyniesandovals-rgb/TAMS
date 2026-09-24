using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Tams.Negocio.Infrastructura.Persistence;

/// <summary>
/// Fábrica design-time para herramientas de migración (dotnet ef).
/// RD-10: la cadena de conexión se lee de configuración o variables de entorno y
/// nunca se escribe en el repositorio. Variable esperada: TAMS_NEGOCIO_CONNECTION_STRING.
/// RD-03: el módulo usa el esquema "negocio" y su propia tabla de historial de migraciones.
/// </summary>
public class TamsDbContextFactory : IDesignTimeDbContextFactory<TamsDbContext>
{
    public TamsDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("TAMS_NEGOCIO_CONNECTION_STRING");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "La variable de entorno TAMS_NEGOCIO_CONNECTION_STRING no está definida. "
                + "RD-10: la cadena de conexión debe venir de configuración/variables de entorno, no del repositorio.");
        }

        var options = new DbContextOptionsBuilder<TamsDbContext>()
            .UseSqlServer(connectionString, sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "negocio"))
            .Options;

        // RD-11/RD-12: las fechas se sellan con el reloj del sistema en UTC vía TimeProvider.
        return new TamsDbContext(options, TimeProvider.System);
    }
}