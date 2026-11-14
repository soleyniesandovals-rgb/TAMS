using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tams.Negocio.Domain.Entidades;

namespace Tams.Negocio.Infrastructura.Persistence.Configuracion;

public class EstudianteConfiguracion : IEntityTypeConfiguration<Estudiante>
{
    public void Configure(EntityTypeBuilder<Estudiante> builder)
    {
        builder.ToTable("Estudiantes", t =>
        {
            // RD-07: la cédula, si existe, debe tener exactamente 11 dígitos.
            t.HasCheckConstraint(
                "CK_Estudiante_Cedula_OnceDigitos",
                "[Cedula] IS NULL OR (LEN([Cedula]) = 11 AND [Cedula] NOT LIKE '%[^0-9]%')");
        });
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Nombre)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.Apellido)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.Cedula)
            .HasMaxLength(11);

        // No se repite una cédula entre estudiantes; los nulos quedan fuera del índice.
        builder.HasIndex(e => e.Cedula)
            .IsUnique()
            .HasFilter("[Cedula] IS NOT NULL");

        builder.Property(e => e.FechaCreacion)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.HasOne(e => e.Curso)
            .WithMany(c => c.Estudiantes)
            .HasForeignKey(e => e.CursoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}