using Core.ControlAcceso.Domain.Entidades;
using Core.ControlAcceso.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core.ControlAcceso.Infrastructura.Persistence.Configuracion;

public class CorreoEnColaConfiguracion : IEntityTypeConfiguration<CorreoEnCola>
{
    public void Configure(EntityTypeBuilder<CorreoEnCola> builder)
    {
        builder.ToTable("CorreosEnCola");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Destinatario)
            .HasMaxLength(320)
            .IsRequired();

        builder.Property(c => c.Asunto)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.Cuerpo)
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(c => c.Estado)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired()
            .HasDefaultValue(EstadoCorreo.Pendiente);

        builder.Property(c => c.FechaCreacion)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(c => c.FechaEnvio)
            .HasColumnType("datetime2");

        // Para que el futuro procesador de la cola pueda tomar los pendientes.
        builder.HasIndex(c => c.Estado);
    }
}