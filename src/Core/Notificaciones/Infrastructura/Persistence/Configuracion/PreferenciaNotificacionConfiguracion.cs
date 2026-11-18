using Core.ControlAcceso.Domain.Entidades;
using Core.Notificaciones.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core.Notificaciones.Infrastructura.Persistence.Configuracion;

/// <summary>
/// Configuración EF de la tabla <c>core.PreferenciasNotificacion</c> (RF-NOT-07).
/// El esquema por defecto lo aporta <c>CoreDbContext</c>.
/// </summary>
public class PreferenciaNotificacionConfiguracion : IEntityTypeConfiguration<PreferenciaNotificacion>
{
    public void Configure(EntityTypeBuilder<PreferenciaNotificacion> builder)
    {
        builder.ToTable("PreferenciasNotificacion");
        builder.HasKey(p => p.UsuarioId);

        builder.Property(p => p.CorreoActivo)
            .IsRequired()
            .HasDefaultValue(true);

        // RF-NOT-07: FK al usuario del Core, sin navegaciones y sin borrado en cascada.
        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(p => p.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
