using Core.Auditoria.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core.Auditoria.Infrastructura.Persistence.Configuracion;

/// <summary>
/// Configuración EF de la tabla <c>core.RegistrosAuditoria</c> (CA-20, RD-01, RD-03).
/// El esquema por defecto lo aporta <c>CoreDbContext</c>.
/// </summary>
public class RegistroAuditoriaConfiguracion : IEntityTypeConfiguration<RegistroAuditoria>
{
    public void Configure(EntityTypeBuilder<RegistroAuditoria> builder)
    {
        builder.ToTable("RegistrosAuditoria");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.UsuarioId)
            .IsRequired();

        // Como EstadoCorreo en la cola: los enums se guardan como texto legible (CA-01).
        builder.Property(r => r.Accion)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(r => r.Entidad)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(r => r.EntidadId)
            .IsRequired();

        // RD-11: la fábrica exige DateTimeKind.Utc; la columna conserva la fecha UTC.
        builder.Property(r => r.FechaHora)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(r => r.ValorAnterior)
            .HasMaxLength(500);

        builder.Property(r => r.ValorNuevo)
            .HasMaxLength(500);

        // CA-15 a CA-18: filtros por fecha, usuario y entidad.
        builder.HasIndex(r => r.FechaHora);
        builder.HasIndex(r => r.UsuarioId);
        builder.HasIndex(r => r.Entidad);
    }
}
