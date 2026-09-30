using Core.ControlAcceso.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core.ControlAcceso.Infrastructura.Persistence.Configuracion;

public class TokenActivacionConfiguracion : IEntityTypeConfiguration<TokenActivacion>
{
    public void Configure(EntityTypeBuilder<TokenActivacion> builder)
    {
        builder.ToTable("TokenActivaciones");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.UsuarioId)
            .IsRequired();

        builder.Property(t => t.Codigo)
            .HasMaxLength(64)
            .IsRequired();
        builder.HasIndex(t => t.Codigo)
            .IsUnique();

        // El reenvío invalida los tokens anteriores buscándolos por usuario (RF-CA-17).
        builder.HasIndex(t => t.UsuarioId);

        builder.Property(t => t.FechaEmision)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(t => t.FechaVencimiento)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(t => t.Usado)
            .IsRequired()
            .HasDefaultValue(false);
    }
}