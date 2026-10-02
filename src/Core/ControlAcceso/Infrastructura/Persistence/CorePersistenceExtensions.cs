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
        services.AddScoped<ICodigosRecuperacionRepositorio, CodigosRecuperacionRepositorio>();
        services.AddScoped<ICoreUnidadDeTrabajo, CoreUnidadDeTrabajo>();

        services.AddSingleton<ITokenGenerador, TokenGeneradorCriptografico>();
        services.AddSingleton<IContrasenaHasher, BcryptContrasenaHasher>();

        services.AddScoped<IRegistroCuentaServicio, RegistroCuentaServicio>();

        services.AddScoped<IActivacionCuentaServicio, ActivacionCuentaServicio>();

        services.AddScoped<IReenvioActivacionServicio, ReenvioActivacionServicio>();

        services.AddScoped<IAutenticacionServicio, AutenticacionServicio>();

        services.AddScoped<ISesionServicio, SesionServicio>();

        services.AddScoped<IAdministracionCuentasServicio, AdministracionCuentasServicio>();

        // RF-CA-03/RD-10: el emisor de JWT se construye leyendo el secreto de firma de
        // variables de entorno (TAMS_JWT_SECRETO), nunca de appsettings.
        services.AddScoped<ITokenJwtGenerador>(sp =>
            new JwtTokenGenerador(sp.GetRequiredService<TimeProvider>(), OpcionesJwt.SecretoDesdeVariableEntorno()));

        return services;
    }

    /// <summary>
    /// Registra el procesador de la cola de correos y el enviador SMTP, leyendo las
    /// credenciales de variables de entorno (RD-10, RF-NOT-13). Solo debe invocarse
    /// donde vaya a procesarse la cola (comando de consola), nunca en el arranque
    /// habitual de la API web: la web no debe exigir credenciales SMTP ni resolver
    /// el procesador por petición (RD-12).
    /// </summary>
    public static IServiceCollection AddProcesadorCorreosPendientes(this IServiceCollection services)
    {
        services.AddScoped<IProcesadorCorreoEnCola, ProcesadorCorreoEnCola>();

        services.AddSingleton<IEnviadorCorreo>(
            _ => new SmtpEnviadorCorreo(OpcionesSmtp.DesdeVariablesEntorno()));

        return services;
    }
}