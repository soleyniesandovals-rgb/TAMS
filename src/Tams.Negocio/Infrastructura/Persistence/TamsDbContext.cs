using Microsoft.EntityFrameworkCore;
using Tams.Negocio.Domain.Entidades;
using Tams.Negocio.Domain.Interfaces;

namespace Tams.Negocio.Infrastructura.Persistence;

/// <summary>
/// DbContext propio del módulo de negocio.
/// No conoce la cadena de conexión: se recibe por <c>DbContextOptions</c> desde configuración
/// o variables de entorno (RD-10).
/// </summary>
public class TamsDbContext : DbContext
{
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Constructor de conveniencia para escenarios sin contenedor de DI
    /// (fábrica design-time, pruebas): usa el reloj del sistema en UTC.
    /// </summary>
    public TamsDbContext(DbContextOptions<TamsDbContext> options)
        : this(options, TimeProvider.System)
    {
    }

    /// <summary>
    /// Constructor principal: recibe el <see cref="TimeProvider"/> inyectado
    /// para sellar las fechas de creación en UTC (RD-11, RD-12).
    /// </summary>
    public TamsDbContext(DbContextOptions<TamsDbContext> options, TimeProvider timeProvider)
        : base(options)
    {
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public DbSet<Centro> Centros => Set<Centro>();

    public DbSet<Materia> Materias => Set<Materia>();

    public DbSet<Curso> Cursos => Set<Curso>();

    public DbSet<AsignacionDocente> AsignacionesDocentes => Set<AsignacionDocente>();

    public DbSet<BloqueHorario> BloquesHorarios => Set<BloqueHorario>();

    public DbSet<Estudiante> Estudiantes => Set<Estudiante>();

    public DbSet<Calificacion> Calificaciones => Set<Calificacion>();

    public DbSet<AnioEscolar> AniosEscolares => Set<AnioEscolar>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Esquema propio del módulo para no chocar con el futuro DbContext del Core (RD-03).
        modelBuilder.HasDefaultSchema("negocio");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TamsDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        AplicarFechaCreacionUtc();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        AplicarFechaCreacionUtc();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    /// <summary>
    /// RD-11/RD-12: la fecha de creación se toma del <see cref="TimeProvider"/> inyectado (UTC).
    /// El código del módulo no usa DateTime.Now ni DateTime.UtcNow directamente,
    /// lo que permite probar y controlar el reloj desde fuera.
    /// </summary>
    private void AplicarFechaCreacionUtc()
    {
        var ahoraUtc = _timeProvider.GetUtcNow().UtcDateTime;

        foreach (var entrada in ChangeTracker.Entries<ICreacionAuditable>())
        {
            if (entrada.State == EntityState.Added)
            {
                entrada.Entity.FechaCreacion = ahoraUtc;
            }
        }
    }
}