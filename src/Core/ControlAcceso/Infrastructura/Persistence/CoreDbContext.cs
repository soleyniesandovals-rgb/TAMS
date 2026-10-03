using Core.ControlAcceso.Domain.Entidades;
using Core.ControlAcceso.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Core.ControlAcceso.Infrastructura.Persistence;

/// <summary>
/// DbContext del Core (esquema "core"). Patrón idéntico a <c>TamsDbContext</c>:
/// no conoce la cadena de conexión (RD-10) y sella las fechas de creación en UTC
/// con el <see cref="TimeProvider"/> inyectado (RD-11, RD-12).
/// </summary>
public class CoreDbContext : DbContext
{
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Constructor de conveniencia para escenarios sin contenedor de DI
    /// (fábrica design-time, pruebas): usa el reloj del sistema en UTC.
    /// </summary>
    public CoreDbContext(DbContextOptions<CoreDbContext> options)
        : this(options, TimeProvider.System)
    {
    }

    /// <summary>
    /// Constructor principal: recibe el <see cref="TimeProvider"/> inyectado
    /// para sellar las fechas de creación en UTC (RD-11, RD-12).
    /// </summary>
    public CoreDbContext(DbContextOptions<CoreDbContext> options, TimeProvider timeProvider)
        : base(options)
    {
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();

    public DbSet<TokenActivacion> TokenActivaciones => Set<TokenActivacion>();

    public DbSet<CorreoEnCola> CorreosEnCola => Set<CorreoEnCola>();

    public DbSet<CodigoRecuperacion> CodigosRecuperacion => Set<CodigoRecuperacion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // RD-03: esquema propio del Core para no chocar con el DbContext del módulo de negocio.
        modelBuilder.HasDefaultSchema("core");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CoreDbContext).Assembly);
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
    /// El código del Core no usa DateTime.Now ni DateTime.UtcNow directamente.
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