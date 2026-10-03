using Core.ControlAcceso.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core.ControlAcceso.Infrastructura.Persistence.Configuracion;

public class CodigoRecuperacionConfiguracion : IEntityTypeConfiguration<CodigoRecuperacion>
{
    public void Configure(EntityTypeBuilder<CodigoRecuperacion> builder)
    {
        builder.ToTable("CodigosRecuperacion");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.UsuarioId)
            .IsRequired();

        builder.Property(c => c.Codigo)
            .HasMaxLength(64)
            .IsRequired();
        builder.HasIndex(c => c.Codigo)
            .IsUnique();

        // Al pedir un código nuevo se invalidan los anteriores buscándolos por usuario.
        builder.HasIndex(c => c.UsuarioId);

        builder.Property(c => c.FechaEmision)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(c => c.FechaVencimiento)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(c => c.Usado)
            .IsRequired()
            .HasDefaultValue(false);
    }
}
