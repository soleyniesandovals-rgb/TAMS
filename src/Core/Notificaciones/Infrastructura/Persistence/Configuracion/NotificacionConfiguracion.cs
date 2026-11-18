using Core.ControlAcceso.Domain.Entidades;
using Core.Notificaciones.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core.Notificaciones.Infrastructura.Persistence.Configuracion;

/// <summary>
/// Configuración EF de la tabla <c>core.Notificaciones</c> (RF-NOT-04).
/// El esquema por defecto lo aporta <c>CoreDbContext</c>.
/// </summary>
public class NotificacionConfiguracion : IEntityTypeConfiguration<Notificacion>
{
    public void Configure(EntityTypeBuilder<Notificacion> builder)
    {
        builder.ToTable("Notificaciones");
        builder.HasKey(n => n.Id);

        builder.Property(n => n.UsuarioId)
            .IsRequired();

        builder.Property(n => n.Tipo)
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(n => n.Titulo)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(n => n.Mensaje)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(n => n.Leida)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(n => n.FechaCreacion)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(n => n.FechaLectura)
            .HasColumnType("datetime2");

        builder.HasIndex(n => new { n.UsuarioId, n.FechaCreacion });

        // RF-NOT-04: FK al usuario del Core, sin navegaciones y sin borrado en cascada.
        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(n => n.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
