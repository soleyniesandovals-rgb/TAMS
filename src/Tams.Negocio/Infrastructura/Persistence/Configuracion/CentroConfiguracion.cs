using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tams.Negocio.Domain.Entidades;
using Tams.Negocio.Domain.Enums;

namespace Tams.Negocio.Infrastructura.Persistence.Configuracion;

public class CentroConfiguracion : IEntityTypeConfiguration<Centro>
{
    public void Configure(EntityTypeBuilder<Centro> builder)
    {
        builder.ToTable("Centros");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Nombre)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(c => c.TipoCentro)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        // Colección de primitivos almacenada como texto separado por comas
        // (por ejemplo: "Software,Redes"), compatible con cualquier proveedor.
        builder.Property(c => c.TiposTecnicosHabilitados)
            .HasConversion(
                v => string.Join(',', v.Select(t => t.ToString())),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                      .Select(s => Enum.Parse<TipoTecnico>(s))
                      .ToList())
            .HasMaxLength(200);

        builder.Property(c => c.FechaCreacion)
            .HasColumnType("datetime2")
            .IsRequired();
    }
}