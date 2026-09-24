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
            .HasMaxLength(50);

        builder.Property(c => c.FechaCreacion)
            .HasColumnType("datetime2")
            .IsRequired();

        // Un mismo grado y sección no pueden repetirse.
        builder.HasIndex(c => new { c.Grado, c.Seccion })
            .IsUnique();
    }
}