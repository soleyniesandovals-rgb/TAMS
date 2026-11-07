using Core.Documentos.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core.Documentos.Infrastructura.Persistence.Configuracion;

/// <summary>
/// Configuración EF de la tabla <c>core.Documentos</c> (RF-DOC-01).
/// El esquema por defecto lo aporta <c>CoreDbContext</c>.
/// </summary>
public class DocumentoConfiguracion : IEntityTypeConfiguration<Documento>
{
    public void Configure(EntityTypeBuilder<Documento> builder)
    {
        builder.ToTable("Documentos");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Nombre)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(d => d.TipoContenido)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(d => d.TamanoBytes)
            .IsRequired();

        builder.Property(d => d.FechaSubida)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(d => d.UsuarioId)
            .IsRequired();

        builder.Property(d => d.RutaAlmacenamiento)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(d => d.Eliminado)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(d => d.UsuarioId);

        // RF-DOC-01: un documento pertenece a un usuario. Igual que en
        // SolicitudPermiso, la FK va con Restrict para no borrar usuarios en cascada.
        builder.HasOne(d => d.Usuario)
            .WithMany()
            .HasForeignKey(d => d.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
