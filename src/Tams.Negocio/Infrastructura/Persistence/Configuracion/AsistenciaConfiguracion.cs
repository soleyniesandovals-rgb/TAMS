using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tams.Negocio.Domain.Entidades;

namespace Tams.Negocio.Infrastructura.Persistence.Configuracion;

public class AsistenciaConfiguracion : IEntityTypeConfiguration<Asistencia>
{
    public void Configure(EntityTypeBuilder<Asistencia> builder)
    {
        builder.ToTable("Asistencias");
        builder.HasKey(a => a.Id);

        // Fecha de calendario de la clase: se mapea a una columna date.
        builder.Property(a => a.Fecha)
            .IsRequired();

        builder.Property(a => a.Estado)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(a => a.Observacion)
            .HasMaxLength(200);

        builder.Property(a => a.FechaCreacion)
            .HasColumnType("datetime2")
            .IsRequired();

        // Un estudiante solo tiene una asistencia por clase y fecha.
        builder.HasIndex(a => new { a.AsignacionDocenteId, a.EstudianteId, a.Fecha })
            .IsUnique();

        // Borrar una asignación o un estudiante no arrastra sus asistencias: primero hay que tratarlas.
        builder.HasOne(a => a.AsignacionDocente)
            .WithMany(d => d.Asistencias)
            .HasForeignKey(a => a.AsignacionDocenteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Estudiante)
            .WithMany(e => e.Asistencias)
            .HasForeignKey(a => a.EstudianteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
