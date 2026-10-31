using Core.ControlAcceso.Domain.Entidades;
using Core.ControlAcceso.Domain.Enums;
using Core.ControlAcceso.Infrastructura.Persistence;
using Core.ControlAcceso.Infrastructura.Repositorios;
using Core.Permisos.Application.Servicios;
using Microsoft.EntityFrameworkCore;
using Tams.Negocio.Application.Servicios;
using Tams.Negocio.Domain.Entidades;
using Tams.Negocio.Domain.Enums;
using Tams.Negocio.Infrastructura.Persistence;

namespace Tests.Integracion;

/// <summary>
/// Contexto de integración sin contenedor de DI ni aplicación levantada. Construye los
/// servicios y repositorios reales del Core y del módulo de negocio sobre dos bases
/// InMemory distintas, únicas por prueba.
/// </summary>
public sealed class ContextoIntegracion : IAsyncDisposable
{
    public const int AdministradorId = 1;
    public const int DocenteId = 10;
    public const int CalificacionId = 1;
    public const decimal PuntajeInicial = 85m;

    private ContextoIntegracion(
        CoreDbContext coreContext,
        TamsDbContext tamsContext,
        CoreUnidadDeTrabajo unidadDeTrabajo,
        GestionPermisosServicio gestionPermisos,
        CalificacionesServicio calificaciones)
    {
        CoreContext = coreContext;
        TamsContext = tamsContext;
        UnidadDeTrabajo = unidadDeTrabajo;
        GestionPermisos = gestionPermisos;
        Calificaciones = calificaciones;
    }

    public CoreDbContext CoreContext { get; }

    public TamsDbContext TamsContext { get; }

    public CoreUnidadDeTrabajo UnidadDeTrabajo { get; }

    public GestionPermisosServicio GestionPermisos { get; }

    public CalificacionesServicio Calificaciones { get; }

    public static async Task<ContextoIntegracion> CrearAsync()
    {
        var sufijo = Guid.NewGuid().ToString("N");

        var coreOptions = new DbContextOptionsBuilder<CoreDbContext>()
            .UseInMemoryDatabase($"tams-core-{sufijo}")
            .Options;

        var tamsOptions = new DbContextOptionsBuilder<TamsDbContext>()
            .UseInMemoryDatabase($"tams-negocio-{sufijo}")
            .Options;

        var coreContext = new CoreDbContext(coreOptions, TimeProvider.System);
        var tamsContext = new TamsDbContext(tamsOptions, TimeProvider.System);

        var unidadDeTrabajo = new CoreUnidadDeTrabajo(coreContext);
        var gestionPermisos = new GestionPermisosServicio(unidadDeTrabajo, TimeProvider.System);
        var calificaciones = new CalificacionesServicio(tamsContext, gestionPermisos);

        await SembrarCoreAsync(coreContext);
        await SembrarNegocioAsync(tamsContext);

        return new ContextoIntegracion(
            coreContext,
            tamsContext,
            unidadDeTrabajo,
            gestionPermisos,
            calificaciones);
    }

    public Task<decimal> ObtenerPuntajeAsync(CancellationToken cancellationToken = default) =>
        TamsContext.Calificaciones
            .AsNoTracking()
            .Where(c => c.Id == CalificacionId)
            .Select(c => c.PuntajeObtenido)
            .SingleAsync(cancellationToken);

    public Task<EstadoCalificacion> ObtenerEstadoAsync(CancellationToken cancellationToken = default) =>
        TamsContext.Calificaciones
            .AsNoTracking()
            .Where(c => c.Id == CalificacionId)
            .Select(c => c.Estado)
            .SingleAsync(cancellationToken);

    public ValueTask DisposeAsync()
    {
        UnidadDeTrabajo.Dispose();
        return TamsContext.DisposeAsync();
    }

    private static async Task SembrarCoreAsync(CoreDbContext context)
    {
        context.Usuarios.AddRange(
            new Usuario
            {
                Id = AdministradorId,
                Nombre = "Administrador de prueba",
                Correo = "administrador@tams.test",
                ContraseñaHash = "hash-de-prueba",
                Rol = RolUsuario.Administrador,
                Activo = true,
            },
            new Usuario
            {
                Id = DocenteId,
                Nombre = "Docente de prueba",
                Correo = "docente@tams.test",
                ContraseñaHash = "hash-de-prueba",
                Rol = RolUsuario.Estandar,
                Activo = true,
            });

        await context.SaveChangesAsync();
    }

    private static async Task SembrarNegocioAsync(TamsDbContext context)
    {
        var anioEscolar = new AnioEscolar
        {
            Id = 1,
            Nombre = "2026-2027",
            FechaInicio = new DateOnly(2026, 1, 1),
            FechaFin = new DateOnly(2026, 12, 31),
            EsActivo = true,
        };

        var curso = new Curso
        {
            Id = 1,
            Grado = "1ro",
            Seccion = "A",
            EsTecnico = false,
            AnioEscolarId = anioEscolar.Id,
            AnioEscolar = anioEscolar,
        };

        var materia = new Materia
        {
            Id = 1,
            Nombre = "Matemática",
            EsTecnica = false,
        };

        var estudiante = new Estudiante
        {
            Id = 1,
            Nombre = "Estudiante",
            Apellido = "De Prueba",
            CursoId = curso.Id,
            Curso = curso,
        };

        var asignacion = new AsignacionDocente
        {
            Id = 1,
            UsuarioId = DocenteId,
            MateriaId = materia.Id,
            CursoId = curso.Id,
            HorasSemanales = 4,
            Materia = materia,
            Curso = curso,
        };

        var calificacion = new Calificacion
        {
            Id = CalificacionId,
            EstudianteId = estudiante.Id,
            Estudiante = estudiante,
            MateriaId = materia.Id,
            Materia = materia,
            AnioEscolarId = anioEscolar.Id,
            AnioEscolar = anioEscolar,
            TipoEvaluacion = TipoEvaluacion.Periodo,
            Periodo = 1,
            PuntajeObtenido = PuntajeInicial,
            Estado = EstadoCalificacion.Publicada,
        };

        context.AddRange(anioEscolar, curso, materia, estudiante, asignacion, calificacion);
        await context.SaveChangesAsync();
    }
}
