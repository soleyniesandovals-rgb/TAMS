using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tams.Negocio.Domain.Entidades;
using Tams.Negocio.Domain.Enums;

namespace Tams.Negocio.Infrastructura.Persistence.Configuracion;

public class CalificacionConfiguracion : IEntityTypeConfiguration<Calificacion>
{
    public void Configure(EntityTypeBuilder<Calificacion> builder)
    {
        builder.ToTable("Calificaciones", t =>
        {
            // Los puntajes nunca pueden ser negativos.
            t.HasCheckConstraint(
                "CK_Calificacion_PuntajeObtenido_NoNegativo",
                "[PuntajeObtenido] >= 0");

            t.HasCheckConstraint(
                "CK_Calificacion_PuntajeRecuperacion_NoNegativo",
                "[PuntajeRecuperacion] IS NULL OR [PuntajeRecuperacion] >= 0");
        });
        builder.HasKey(c => c.Id);

        builder.Property(c => c.TipoEvaluacion)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(c => c.Periodo);

        builder.Property(c => c.NumeroRA);

        builder.Property(c => c.PuntajeObtenido)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(c => c.PuntajeRecuperacion)
            .HasPrecision(18, 2);

        // RD-04: el estado vive en un único enum; por defecto una calificación nace en Borrador.
        builder.Property(c => c.Estado)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired()
            .HasDefaultValue(EstadoCalificacion.Borrador);

        builder.Property(c => c.FechaCreacion)
            .HasColumnType("datetime2")
            .IsRequired();

        // Borrar un estudiante no arrastra sus calificaciones: primero hay que tratarlas.
        builder.HasOne(c => c.Estudiante)
            .WithMany(e => e.Calificaciones)
            .HasForeignKey(c => c.EstudianteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Materia)
            .WithMany(m => m.Calificaciones)
            .HasForeignKey(c => c.MateriaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}