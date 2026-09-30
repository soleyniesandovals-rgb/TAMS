using Core.ControlAcceso.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core.ControlAcceso.Infrastructura.Persistence.Configuracion;

public class UsuarioConfiguracion : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Nombre)
            .HasMaxLength(150)
            .IsRequired();

        // RF-CA-01: el correo es único. El servicio además lo normaliza a minúsculas.
        builder.Property(u => u.Correo)
            .HasMaxLength(320)
            .IsRequired();
        builder.HasIndex(u => u.Correo)
            .IsUnique();

        // RF-CA-02: nunca se guarda la contraseña en texto plano; solo su hash BCrypt (60 caracteres).
        builder.Property(u => u.ContraseñaHash)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.Rol)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        // RF-CA-15: la cuenta nace inactiva.
        builder.Property(u => u.Activo)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(u => u.FechaCreacion)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.HasMany(u => u.TokensActivacion)
            .WithOne(t => t.Usuario)
            .HasForeignKey(t => t.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}