using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tams.Negocio.Domain.Entidades;

namespace Tams.Negocio.Infrastructura.Persistence.Configuracion;

public class CursoConfiguracion : IEntityTypeConfiguration<Curso>
{
    public void Configure(EntityTypeBuilder<Curso> builder)
    {
        builder.ToTable("Cursos", t =>
        {
            // RF-NEG-01: el tipo técnico solo aplica a cursos técnicos.
            t.HasCheckConstraint(
                "CK_Curso_TipoTecnico_SoloSiEsTecnico",
                "[TipoTecnico] IS NULL OR [EsTecnico] = 1");
        });
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Grado)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(c => c.Seccion)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(c => c.EsTecnico)
            .IsRequired();

        builder.Property(c => c.TipoTecnico)
            .HasConversion<string>()
            // El nombre más largo del enum supera los 50 caracteres; se reserva margen.
            .HasMaxLength(100);

        builder.Property(c => c.FechaCreacion)
            .HasColumnType("datetime2")
            .IsRequired();

        // No se borra un año escolar que tenga cursos: primero hay que tratarlos.
        builder.HasOne(c => c.AnioEscolar)
            .WithMany(a => a.Cursos)
            .HasForeignKey(c => c.AnioEscolarId)
            .OnDelete(DeleteBehavior.Restrict);

        // Un mismo grado y sección no pueden repetirse dentro de un año escolar.
        builder.HasIndex(c => new { c.AnioEscolarId, c.Grado, c.Seccion })
            .IsUnique();
    }
}