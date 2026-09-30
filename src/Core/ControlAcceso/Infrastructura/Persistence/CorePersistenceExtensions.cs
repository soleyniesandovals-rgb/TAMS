using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Core.ControlAcceso.Application.Interfaces;
using Core.ControlAcceso.Application.Servicios;
using Core.ControlAcceso.Infrastructura.Repositorios;
using Core.ControlAcceso.Infrastructura.Servicios;

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

    /// <summary>
    /// Registra los servicios, repositorios y adaptadores del módulo de control de acceso.
    /// Cada pieza queda inyectable por interfaz para poder probarse sin levantar la
    /// aplicación completa (RD-12).
    /// </summary>
    public static IServiceCollection AddCoreControlAcceso(this IServiceCollection services)
    {
        services.AddScoped<IUsuariosRepositorio, UsuariosRepositorio>();
        services.AddScoped<ITokenActivacionesRepositorio, TokenActivacionesRepositorio>();
        services.AddScoped<ICorreosEnColaRepositorio, CorreosEnColaRepositorio>();
        services.AddScoped<ICoreUnidadDeTrabajo, CoreUnidadDeTrabajo>();

        services.AddSingleton<ITokenGenerador, TokenGeneradorCriptografico>();
        services.AddSingleton<IContrasenaHasher, BcryptContrasenaHasher>();

        services.AddScoped<IRegistroCuentaServicio, RegistroCuentaServicio>();

        services.AddScoped<IActivacionCuentaServicio, ActivacionCuentaServicio>();

        return services;
    }
}