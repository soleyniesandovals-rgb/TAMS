using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Core.Auditoria.Application.Interfaces;
using Core.Auditoria.Application.Servicios;
using Core.ControlAcceso.Application.Interfaces;
using Core.ControlAcceso.Application.Servicios;
using Core.ControlAcceso.Infrastructura.Repositorios;
using Core.ControlAcceso.Infrastructura.Servicios;
using Core.Documentos.Application.Interfaces;
using Core.Documentos.Application.Servicios;
using Core.Documentos.Infrastructura.Repositorios;
using Core.Documentos.Infrastructura.Servicios;
using Core.Notificaciones.Application.Interfaces;
using Core.Notificaciones.Application.Servicios;
using Core.Notificaciones.Infrastructura.Repositorios;
using Core.Permisos.Application.Interfaces;
using Core.Permisos.Application.Servicios;
using Core.Permisos.Infrastructura.Repositorios;
using Core.Reportes.Application.Interfaces;
using Core.Reportes.Application.Servicios;

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

        services.AddScoped<IConsultaUsuariosServicio, ConsultaUsuariosServicio>();

        services.AddScoped<IConsultaCorreosEnColaServicio, ConsultaCorreosEnColaServicio>();

        services.AddScoped<IRecuperacionContrasenaServicio, RecuperacionContrasenaServicio>();

        services.AddScoped<ICambioContrasenaServicio, CambioContrasenaServicio>();

        // RF-CA-03/RD-10: el emisor de JWT se construye leyendo el secreto de firma de
        // variables de entorno (TAMS_JWT_SECRETO), nunca de appsettings.
        services.AddScoped<ITokenJwtGenerador>(sp =>
            new JwtTokenGenerador(sp.GetRequiredService<TimeProvider>(), OpcionesJwt.SecretoDesdeVariableEntorno()));

        return services;
    }

    /// <summary>
    /// Registra el servicio y los repositorios de la pieza de permisos (RF-GP-01 a
    /// RF-GP-07). Vive aquí porque <see cref="CorePersistenceExtensions"/> es el punto de
    /// entrada único de TODO el Core, no solo de una pieza (RD-12). Cada componente queda
    /// inyectable por interfaz y como Scoped, igual que los de control de acceso.
    /// </summary>
    public static IServiceCollection AddCorePermisos(this IServiceCollection services)
    {
        services.AddScoped<ISolicitudesPermisoRepositorio, SolicitudesPermisoRepositorio>();
        services.AddScoped<IPermisosActivosRepositorio, PermisosActivosRepositorio>();
        services.AddScoped<IGestionPermisosServicio, GestionPermisosServicio>();

        return services;
    }

    /// <summary>
    /// Registra el servicio y los repositorios de documentos (RF-DOC-01 a RF-DOC-04).
    /// El almacenamiento local se resuelve como singleton y crea su carpeta base al
    /// construirse.
    /// </summary>
    public static IServiceCollection AddCoreDocumentos(this IServiceCollection services)
    {
        services.AddScoped<IDocumentosRepositorio, DocumentosRepositorio>();
        services.AddScoped<IGestionDocumentosServicio, GestionDocumentosServicio>();
        services.AddSingleton<IAlmacenamientoArchivos, AlmacenamientoArchivosLocal>();

        return services;
    }

    /// <summary>
    /// Registra el servicio de reportes del Core (RF-REP-01, RF-REP-04). El
    /// repositorio que usa ya está registrado en <see cref="AddCorePermisos"/>,
    /// porque la pieza Permisos es la dueña de los datos (RD-01).
    /// </summary>
    public static IServiceCollection AddCoreReportes(this IServiceCollection services)
    {
        services.AddScoped<IReporteSolicitudesPermisoServicio, ReporteSolicitudesPermisoServicio>();

        return services;
    }

    /// <summary>
    /// Registra el registrador de auditoría (RF-AUD-01). El repositorio NO se registra
    /// suelto: <see cref="ICoreUnidadDeTrabajo"/> lo construye sobre el mismo
    /// <see cref="CoreDbContext"/> para que el registro se guarde con la operación
    /// auditada (CA-12). Todavía no se conecta a ningún evento.
    /// </summary>
    public static IServiceCollection AddCoreAuditoria(this IServiceCollection services)
    {
        services.AddScoped<IRegistradorAuditoria, RegistradorAuditoria>();

        return services;
    }

    /// <summary>
    /// Registra los servicios de la pieza de notificaciones (RF-NOT-04 a RF-NOT-07).
    /// Los repositorios NO se registran sueltos: <see cref="ICoreUnidadDeTrabajo"/> los
    /// construye sobre el mismo <see cref="CoreDbContext"/> para garantizar una sola
    /// transacción (C-04). Las demás piezas inyectan solo <see cref="INotificador"/>.
    /// </summary>
    public static IServiceCollection AddCoreNotificaciones(this IServiceCollection services)
    {
        services.AddScoped<INotificador, Notificador>();
        services.AddScoped<INotificacionesServicio, NotificacionesServicio>();

        return services;
    }

    /// <summary>
    /// Registra el procesador de la cola de correos y el enviador. Si TAMS_SMTP_HOST
    /// no está definida o está vacía, usa <see cref="EnviadorCorreoLog"/> (RD-10);
    /// si tiene valor, exige las credenciales SMTP de variables de entorno
    /// (RF-NOT-13). Solo debe invocarse donde vaya a procesarse la cola (comando de
    /// consola), nunca en el arranque habitual de la API web: la web no debe exigir
    /// credenciales SMTP ni resolver el procesador por petición (RD-12).
    /// </summary>
    public static IServiceCollection AddProcesadorCorreosPendientes(this IServiceCollection services)
    {
        services.AddScoped<IProcesadorCorreoEnCola, ProcesadorCorreoEnCola>();

        services.AddSingleton<IEnviadorCorreo>(sp =>
        {
            var host = Environment.GetEnvironmentVariable("TAMS_SMTP_HOST");

            return string.IsNullOrWhiteSpace(host)
                ? new EnviadorCorreoLog(sp.GetRequiredService<ILogger<EnviadorCorreoLog>>())
                : new SmtpEnviadorCorreo(OpcionesSmtp.DesdeVariablesEntorno());
        });

        return services;
    }
}