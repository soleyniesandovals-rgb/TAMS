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
    public TamsDbContext(DbContextOptions<TamsDbContext> options)
        : base(options)
    {
    }

    public DbSet<Centro> Centros => Set<Centro>();

    public DbSet<Materia> Materias => Set<Materia>();

    public DbSet<Curso> Cursos => Set<Curso>();

    public DbSet<AsignacionDocente> AsignacionesDocentes => Set<AsignacionDocente>();

    public DbSet<BloqueHorario> BloquesHorarios => Set<BloqueHorario>();

    public DbSet<Estudiante> Estudiantes => Set<Estudiante>();

    public DbSet<Calificacion> Calificaciones => Set<Calificacion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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
    /// RD-11: las fechas de creación se registran en UTC (DateTime.UtcNow).
    /// Nunca se usa DateTime.Now, que devuelve la hora local del servidor.
    /// </summary>
    private void AplicarFechaCreacionUtc()
    {
        foreach (var entrada in ChangeTracker.Entries<ICreacionAuditable>())
        {
            if (entrada.State == EntityState.Added)
            {
                entrada.Entity.FechaCreacion = DateTime.UtcNow;
            }
        }
    }
}