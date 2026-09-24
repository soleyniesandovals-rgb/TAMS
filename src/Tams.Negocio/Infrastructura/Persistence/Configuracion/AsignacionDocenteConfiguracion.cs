using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tams.Negocio.Domain.Entidades;

namespace Tams.Negocio.Infrastructura.Persistence.Configuracion;

public class AsignacionDocenteConfiguracion : IEntityTypeConfiguration<AsignacionDocente>
{
    public void Configure(EntityTypeBuilder<AsignacionDocente> builder)
    {
        builder.ToTable("AsignacionesDocentes", t =>
        {
            t.HasCheckConstraint(
                "CK_AsignacionDocente_HorasSemanales_Positivas",
                "[HorasSemanales] > 0");
        });
        builder.HasKey(a => a.Id);

        // RD-03: el docente es un usuario del Core; aquí solo existe el Id, sin FK.
        builder.Property(a => a.UsuarioId)
            .IsRequired();

        builder.Property(a => a.HorasSemanales)
            .IsRequired();

        builder.Property(a => a.FechaCreacion)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.HasOne(a => a.Materia)
            .WithMany(m => m.AsignacionesDocentes)
            .HasForeignKey(a => a.MateriaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Curso)
            .WithMany(c => c.AsignacionesDocentes)
            .HasForeignKey(a => a.CursoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Un docente no puede tener dos asignaciones idénticas (misma materia y curso).
        builder.HasIndex(a => new { a.UsuarioId, a.MateriaId, a.CursoId })
            .IsUnique();
    }
}