using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tams.Negocio.Domain.Entidades;

namespace Tams.Negocio.Infrastructura.Persistence.Configuracion;

public class BloqueHorarioConfiguracion : IEntityTypeConfiguration<BloqueHorario>
{
    public void Configure(EntityTypeBuilder<BloqueHorario> builder)
    {
        builder.ToTable("BloquesHorarios", t =>
        {
            // Un bloque siempre termina después de empezar.
            t.HasCheckConstraint(
                "CK_BloqueHorario_HoraFin_MayorQue_HoraInicio",
                "[HoraFin] > [HoraInicio]");
        });
        builder.HasKey(b => b.Id);

        builder.Property(b => b.DiaSemana)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(b => b.HoraInicio)
            .HasColumnType("time")
            .IsRequired();

        builder.Property(b => b.HoraFin)
            .HasColumnType("time")
            .IsRequired();

        builder.Property(b => b.Bloqueado)
            .IsRequired();

        builder.Property(b => b.FechaCreacion)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.HasOne(b => b.AsignacionDocente)
            .WithMany(a => a.BloquesHorarios)
            .HasForeignKey(b => b.AsignacionDocenteId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}