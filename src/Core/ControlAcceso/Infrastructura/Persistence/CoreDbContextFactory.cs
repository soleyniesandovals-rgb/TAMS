using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Core.ControlAcceso.Infrastructura.Persistence;

/// <summary>
/// Fábrica design-time para las herramientas de migración del Core.
/// RD-10: la cadena de conexión se lee de variables de entorno y nunca se escribe
/// en el repositorio. Variable esperada: TAMS_CORE_CONNECTION_STRING.
/// RD-03: el Core usa el esquema "core" y su propia tabla de historial de migraciones.
/// </summary>
public class CoreDbContextFactory : IDesignTimeDbContextFactory<CoreDbContext>
{
    public CoreDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("TAMS_CORE_CONNECTION_STRING");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "La variable de entorno TAMS_CORE_CONNECTION_STRING no está definida. "
                + "RD-10: la cadena de conexión debe venir de configuración/variables de entorno, no del repositorio.");
        }

        var options = new DbContextOptionsBuilder<CoreDbContext>()
            .UseSqlServer(connectionString, sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "core"))
            .Options;

        // RD-11/RD-12: las fechas se sellan con el reloj del sistema en UTC vía TimeProvider.
        return new CoreDbContext(options, TimeProvider.System);
    }
}