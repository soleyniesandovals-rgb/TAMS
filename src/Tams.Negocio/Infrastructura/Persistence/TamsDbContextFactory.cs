using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Tams.Negocio.Infrastructura.Persistence;

/// <summary>
/// Fábrica design-time para herramientas de migración (dotnet ef).
/// RD-10: la cadena de conexión se lee de configuración o variables de entorno y
/// nunca se escribe en el repositorio. Variable esperada: TAMS_NEGOCIO_CONNECTION_STRING.
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
            .UseSqlServer(connectionString)
            .Options;

        return new TamsDbContext(options);
    }
}