using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tams.Negocio.Domain.Entidades;

namespace Tams.Negocio.Infrastructura.Persistence.Configuracion;

public class MateriaConfiguracion : IEntityTypeConfiguration<Materia>
{
    public void Configure(EntityTypeBuilder<Materia> builder)
    {
        builder.ToTable("Materias", t =>
        {
            // RF-NEG-01: tipo técnico y cantidad de RA solo tienen sentido si la materia es técnica.
            t.HasCheckConstraint(
                "CK_Materia_TipoTecnico_SoloSiEsTecnica",
                "[TipoTecnico] IS NULL OR [EsTecnica] = 1");

            t.HasCheckConstraint(
                "CK_Materia_CantidadRA_SoloSiEsTecnica",
                "[CantidadRA] IS NULL OR [EsTecnica] = 1");

            t.HasCheckConstraint(
                "CK_Materia_CantidadRA_Positiva",
                "[CantidadRA] IS NULL OR [CantidadRA] > 0");
        });
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Nombre)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(m => m.EsTecnica)
            .IsRequired();

        builder.Property(m => m.TipoTecnico)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(m => m.CantidadRA);

        builder.Property(m => m.FechaCreacion)
            .HasColumnType("datetime2")
            .IsRequired();
    }
}