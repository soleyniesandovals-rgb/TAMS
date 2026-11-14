using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tams.Negocio.Domain.Entidades;

namespace Tams.Negocio.Infrastructura.Persistence.Configuracion;

public class ResultadoAprendizajeConfiguracion : IEntityTypeConfiguration<ResultadoAprendizaje>
{
    public void Configure(EntityTypeBuilder<ResultadoAprendizaje> builder)
    {
        builder.ToTable("ResultadosAprendizaje", t =>
        {
            // RF-NEG-01: cada RA vale entre 1 y 100 puntos y se numera desde 1.
            t.HasCheckConstraint(
                "CK_ResultadoAprendizaje_Puntos_EntreUnoYCien",
                "[Puntos] BETWEEN 1 AND 100");

            t.HasCheckConstraint(
                "CK_ResultadoAprendizaje_Numero_Positivo",
                "[Numero] >= 1");
        });
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Numero)
            .IsRequired();

        builder.Property(r => r.Descripcion)
            .HasMaxLength(200);

        builder.Property(r => r.Puntos)
            .IsRequired();

        builder.Property(r => r.FechaCreacion)
            .HasColumnType("datetime2")
            .IsRequired();

        // Borrar una materia arrastra sus resultados de aprendizaje.
        builder.HasOne(r => r.Materia)
            .WithMany(m => m.ResultadosAprendizaje)
            .HasForeignKey(r => r.MateriaId)
            .OnDelete(DeleteBehavior.Cascade);

        // Dentro de una materia no se repite el número de RA.
        builder.HasIndex(r => new { r.MateriaId, r.Numero })
            .IsUnique();
    }
}
