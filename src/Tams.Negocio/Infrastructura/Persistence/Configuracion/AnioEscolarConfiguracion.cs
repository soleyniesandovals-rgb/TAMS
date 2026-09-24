using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tams.Negocio.Domain.Entidades;

namespace Tams.Negocio.Infrastructura.Persistence.Configuracion;

public class AnioEscolarConfiguracion : IEntityTypeConfiguration<AnioEscolar>
{
    public void Configure(EntityTypeBuilder<AnioEscolar> builder)
    {
        builder.ToTable("AniosEscolares", t =>
        {
            // RF-NEG-01: el fin del año escolar siempre debe ser posterior a su inicio.
            t.HasCheckConstraint(
                "CK_AnioEscolar_FechaFin_MayorQue_FechaInicio",
                "[FechaFin] > [FechaInicio]");
        });
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Nombre)
            .HasMaxLength(30)
            .IsRequired();
        builder.HasIndex(a => a.Nombre)
            .IsUnique();

        // Fechas de calendario del año escolar: se mapean a columnas date.
        builder.Property(a => a.FechaInicio)
            .IsRequired();

        builder.Property(a => a.FechaFin)
            .IsRequired();

        builder.Property(a => a.EsActivo)
            .IsRequired();

        // RF-NEG-01: solo un año escolar puede estar activo a la vez.
        // Índice único filtrado: solo se indexan las filas con EsActivo = 1.
        builder.HasIndex(a => a.EsActivo)
            .IsUnique()
            .HasFilter("[EsActivo] = 1");

        builder.Property(a => a.FechaCreacion)
            .HasColumnType("datetime2")
            .IsRequired();
    }
}