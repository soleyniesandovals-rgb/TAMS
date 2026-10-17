using Core.Permisos.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core.Permisos.Infrastructura.Persistence.Configuracion;

public class PermisoActivoConfiguracion : IEntityTypeConfiguration<PermisoActivo>
{
    public void Configure(EntityTypeBuilder<PermisoActivo> builder)
    {
        builder.ToTable("PermisosActivos");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.UsuarioId)
            .IsRequired();

        builder.Property(p => p.TipoPermiso)
            .HasMaxLength(100)
            .IsRequired();

        // RF-GP-05: un usuario no puede tener el mismo permiso otorgado dos veces.
        builder.HasIndex(p => new { p.UsuarioId, p.TipoPermiso })
            .IsUnique();

        builder.Property(p => p.SolicitudPermisoId)
            .IsRequired();

        builder.Property(p => p.FechaCreacion)
            .HasColumnType("datetime2")
            .IsRequired();

        // Al borrar el usuario se borran sus permisos otorgados; no deben quedar huérfanos.
        builder.HasOne(p => p.Usuario)
            .WithMany()
            .HasForeignKey(p => p.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        // La solicitud que originó el permiso no se borra mientras el permiso exista (trazabilidad).
        builder.HasOne(p => p.SolicitudPermiso)
            .WithMany()
            .HasForeignKey(p => p.SolicitudPermisoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
