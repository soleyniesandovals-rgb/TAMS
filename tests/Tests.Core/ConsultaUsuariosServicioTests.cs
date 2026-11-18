using Core.ControlAcceso.Application.Interfaces;
using Core.ControlAcceso.Application.Modelos;
using Core.ControlAcceso.Application.Servicios;
using Core.ControlAcceso.Domain.Entidades;
using Core.ControlAcceso.Domain.Enums;
using Xunit;

namespace Tests.Core;

/// <summary>
/// Pruebas de la consulta de solo lectura de usuarios para otros módulos
/// (RF-NEG-02, RD-03, RD-12).
/// </summary>
public class ConsultaUsuariosServicioTests
{
    [Fact]
    public async Task Obtener_UsuarioExistente_DevuelveRolYActivoSinDatosSensibles()
    {
        // Preparar
        var usuario = new Usuario
        {
            Id = 2,
            Nombre = "Docente Ejemplo",
            Correo = "docente@ejemplo.com",
            ContraseñaHash = "hash-de-prueba",
            Rol = RolUsuario.Estandar,
            Activo = true,
        };
        var servicio = new ConsultaUsuariosServicio(new RepositorioUsuariosFalso(usuario));

        // Actuar
        var resumen = await servicio.ObtenerAsync(2);

        // Verificar
        Assert.NotNull(resumen);
        Assert.Equal(2, resumen.Id);
        Assert.Equal("Docente Ejemplo", resumen.Nombre);
        Assert.Equal("docente@ejemplo.com", resumen.Correo);
        Assert.Equal("Estandar", resumen.Rol);
        Assert.True(resumen.Activo);

        var propiedades = typeof(UsuarioConsultaResumen).GetProperties();
        Assert.DoesNotContain(
            propiedades,
            propiedad => propiedad.Name.Contains("Hash", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(
            propiedades,
            propiedad => propiedad.Name.Contains("Contrasena", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Obtener_UsuarioInexistente_DevuelveNull()
    {
        // Preparar
        var servicio = new ConsultaUsuariosServicio(new RepositorioUsuariosFalso());

        // Actuar
        var resumen = await servicio.ObtenerAsync(999);

        // Verificar
        Assert.Null(resumen);
    }

    [Fact]
    public async Task ObtenerVarios_DevuelveSoloLosIdsPedidosQueExisten()
    {
        // Preparar
        var usuarios = new[]
        {
            new Usuario
            {
                Id = 1,
                Nombre = "Administrador",
                Correo = "admin@ejemplo.com",
                Rol = RolUsuario.Administrador,
                Activo = true,
            },
            new Usuario
            {
                Id = 2,
                Nombre = "Docente",
                Correo = "docente@ejemplo.com",
                Rol = RolUsuario.Estandar,
                Activo = false,
            },
        };
        var servicio = new ConsultaUsuariosServicio(new RepositorioUsuariosFalso(usuarios));

        // Actuar
        var resumenes = await servicio.ObtenerVariosAsync([2, 99, 100]);

        // Verificar
        var resumen = Assert.Single(resumenes);
        Assert.Equal(2, resumen.Id);
        Assert.Equal("Estandar", resumen.Rol);
        Assert.False(resumen.Activo);
    }

    private sealed class RepositorioUsuariosFalso : IUsuariosRepositorio
    {
        private readonly List<Usuario> _usuarios;

        public RepositorioUsuariosFalso(params Usuario[] usuarios)
        {
            _usuarios = [.. usuarios];
        }

        public Task<Usuario?> BuscarPorCorreoAsync(
            string correo,
            CancellationToken cancellationToken = default)
            => Task.FromResult(_usuarios.FirstOrDefault(u => u.Correo == correo));

        public Task<Usuario?> BuscarPorIdAsync(
            int id,
            CancellationToken cancellationToken = default)
            => Task.FromResult(_usuarios.FirstOrDefault(u => u.Id == id));

        public Task<IReadOnlyList<Usuario>> BuscarPorIdsAsync(
            IReadOnlyCollection<int> ids,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Usuario>>(
                _usuarios.Where(u => ids.Contains(u.Id)).OrderBy(u => u.Id).ToList());

        public Task<IReadOnlyList<Usuario>> ListarAsync(
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Usuario>>(_usuarios.OrderBy(u => u.Id).ToList());

        public Task<IReadOnlyList<Usuario>> ListarAdministradoresActivosAsync(
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Usuario>>(
                _usuarios.Where(u => u.Rol == RolUsuario.Administrador && u.Activo).OrderBy(u => u.Id).ToList());

        public Task AgregarAsync(
            Usuario usuario,
            CancellationToken cancellationToken = default)
        {
            _usuarios.Add(usuario);
            return Task.CompletedTask;
        }
    }
}
