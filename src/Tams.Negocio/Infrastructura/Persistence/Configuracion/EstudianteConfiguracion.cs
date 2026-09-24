using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tams.Negocio.Domain.Entidades;

namespace Tams.Negocio.Infrastructura.Persistence.Configuracion;

public class EstudianteConfiguracion : IEntityTypeConfiguration<Estudiante>
{
    public void Configure(EntityTypeBuilder<Estudiante> builder)
    {
        builder.ToTable("Estudiantes");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Nombre)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.Apellido)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.FechaCreacion)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.HasOne(e => e.Curso)
            .WithMany(c => c.Estudiantes)
            .HasForeignKey(e => e.CursoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}