using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tams.Negocio.Domain.Entidades;

namespace Tams.Negocio.Infrastructura.Persistence.Configuracion;

/// <summary>
/// Configuración EF de la tabla <c>negocio.EventosCalendario</c> (RF-NEG).
/// El esquema por defecto lo aporta <c>TamsDbContext</c>.
/// </summary>
public class EventoCalendarioConfiguracion : IEntityTypeConfiguration<EventoCalendario>
{
    public void Configure(EntityTypeBuilder<EventoCalendario> builder)
    {
        builder.ToTable("EventosCalendario");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.AnioEscolarId)
            .IsRequired();

        builder.Property(e => e.Fecha)
            .IsRequired();

        builder.Property(e => e.Descripcion)
            .HasMaxLength(200)
            .IsRequired();

        // RD-03: el creador es un usuario del Core; aquí solo existe el Id, sin FK.
        builder.Property(e => e.CreadoPorUsuarioId)
            .IsRequired();

        builder.Property(e => e.FechaCreacion)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.HasOne(e => e.AnioEscolar)
            .WithMany(a => a.EventosCalendario)
            .HasForeignKey(e => e.AnioEscolarId)
            .OnDelete(DeleteBehavior.Restrict);

        // Varios eventos pueden caer el mismo día: el índice no es único.
        builder.HasIndex(e => new { e.AnioEscolarId, e.Fecha });
    }
}
