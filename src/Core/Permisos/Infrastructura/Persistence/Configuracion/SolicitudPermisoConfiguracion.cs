using Core.Permisos.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core.Permisos.Infrastructura.Persistence.Configuracion;

public class SolicitudPermisoConfiguracion : IEntityTypeConfiguration<SolicitudPermiso>
{
    public void Configure(EntityTypeBuilder<SolicitudPermiso> builder)
    {
        builder.ToTable("SolicitudesPermiso");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.SolicitanteId)
            .IsRequired();

        builder.Property(s => s.TipoPermiso)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(s => s.Estado)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(s => s.FechaCreacion)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(s => s.FechaResolucion)
            .HasColumnType("datetime2");

        // Obligatorio solo cuando la solicitud está rechazada; la base no expresa
        // obligatoriedad condicional, así que la regla completa la valida el servicio (Bloque 2).
        builder.Property(s => s.MotivoRechazo)
            .HasMaxLength(500);

        // RF-GP-01: la solicitud la pide un usuario y la resuelve otro. Ambas FK apuntan a
        // Usuarios, así que SQL Server no admite cascada doble: van con Restrict (RD-09).
        builder.HasOne(s => s.Solicitante)
            .WithMany()
            .HasForeignKey(s => s.SolicitanteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.ResueltaPor)
            .WithMany()
            .HasForeignKey(s => s.ResueltaPorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
