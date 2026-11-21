using Core.Auditoria.Application.Servicios;
using Core.Auditoria.Domain.Entidades;
using Core.Auditoria.Domain.Enums;
using Core.ControlAcceso.Application.Interfaces;
using Core.ControlAcceso.Application.Opciones;
using Core.ControlAcceso.Application.Servicios;
using Core.ControlAcceso.Domain.Entidades;
using Core.ControlAcceso.Domain.Enums;
using Core.ControlAcceso.Domain.Excepciones;
using Microsoft.Extensions.Options;
using Tests.Core.Dobles;
using Xunit;

namespace Tests.Core;

/// <summary>
/// RF-AUD-05: auditoría de restablecimientos, recuperaciones y cambios de contraseña
/// (CA-08 a CA-13). Ningún registro contiene contraseñas, hashes ni códigos (CA-11).
/// </summary>
public class ContrasenaAuditoriaTests
{
    private const string CodigoSecreto = "CODIGO-SECRETO-123";
    private const string ContrasenaActual = "ClaveActual!2026";
    private const string ContrasenaNueva = "ClaveNueva!2026";

    [Fact]
    public async Task RF_AUD_05_RestablecerAsync_RegistraLosSeisCampos()
    {
        // CA-08: RestablecerContrasena / Usuario / id del afectado / administrador del token.
        // Preparar: un usuario afectado y un administrador distinto.
        var escenario = await CrearEscenarioAsync();

        // Actuar: el administrador fuerza el restablecimiento.
        await escenario.Recuperacion.RestablecerAsync(5, 3, CancellationToken.None);

        // Verificar: un registro con los seis campos y valor nuevo literal del criterio.
        var registro = Assert.Single(escenario.Unidad.Registros);
        Assert.Equal(3, registro.UsuarioId);
        Assert.Equal(AccionAuditoria.RestablecerContrasena, registro.Accion);
        Assert.Equal(EntidadAuditoria.Usuario, registro.Entidad);
        Assert.Equal(5, registro.EntidadId);
        Assert.Null(registro.ValorAnterior);
        Assert.Equal("Contraseña restablecida por un Administrador", registro.ValorNuevo);
    }

    [Fact]
    public async Task RF_AUD_05_ConfirmarAsync_RegistraLosSeisCampos()
    {
        // CA-09: RecuperarContrasena sin sesión, con el propio usuario del código como actor.
        // Preparar: un código válido no usado.
        var escenario = await CrearEscenarioAsync();
        await SembrarCodigoAsync(escenario);

        // Actuar: se confirma la recuperación con el código y la contraseña nueva.
        await escenario.Recuperacion.ConfirmarAsync(CodigoSecreto, ContrasenaNueva, CancellationToken.None);

        // Verificar: los seis campos; el id del código coincide con el actor.
        var registro = Assert.Single(escenario.Unidad.Registros);
        Assert.Equal(5, registro.UsuarioId);
        Assert.Equal(AccionAuditoria.RecuperarContrasena, registro.Accion);
        Assert.Equal(EntidadAuditoria.Usuario, registro.Entidad);
        Assert.Equal(5, registro.EntidadId);
        Assert.Null(registro.ValorAnterior);
        Assert.Equal("Contraseña restablecida por el propio usuario", registro.ValorNuevo);
    }

    [Fact]
    public async Task RF_AUD_05_CambiarAsync_RegistraLosSeisCampos()
    {
        // CA-10: CambiarContrasena con el propio usuario autenticado como actor.
        // Preparar: un usuario con la contraseña actual conocida.
        var escenario = await CrearEscenarioAsync();

        // Actuar: el usuario cambia su contraseña.
        await escenario.Cambio.CambiarAsync(5, ContrasenaActual, ContrasenaNueva, CancellationToken.None);

        // Verificar: los seis campos, sin valores anteriores.
        var registro = Assert.Single(escenario.Unidad.Registros);
        Assert.Equal(5, registro.UsuarioId);
        Assert.Equal(AccionAuditoria.CambiarContrasena, registro.Accion);
        Assert.Equal(EntidadAuditoria.Usuario, registro.Entidad);
        Assert.Equal(5, registro.EntidadId);
        Assert.Null(registro.ValorAnterior);
        Assert.Equal("Contraseña cambiada por el propio usuario", registro.ValorNuevo);
    }

    [Fact]
    public async Task RF_AUD_05_ConfirmarAsync_RegistroSinContrasenaHashNiCodigo()
    {
        // CA-11: ningún campo del registro de recuperación contiene secretos.
        // Preparar: un código válido y una contraseña nueva conocidos.
        var escenario = await CrearEscenarioAsync();
        await SembrarCodigoAsync(escenario);

        // Actuar: se completa la recuperación.
        await escenario.Recuperacion.ConfirmarAsync(CodigoSecreto, ContrasenaNueva, CancellationToken.None);

        // Verificar: no aparecen la contraseña nueva, su hash ni el código.
        AssertSinSecretos(
            Assert.Single(escenario.Unidad.Registros),
            ContrasenaNueva,
            escenario.Hasher.Hash(ContrasenaNueva),
            CodigoSecreto);
    }

    [Fact]
    public async Task RF_AUD_05_RestablecerAsync_RegistroSinHashNiCodigo()
    {
        // CA-11: el restablecimiento forzado no guarda el código ni hashes en el registro.
        // Preparar: un administrador y un usuario afectado.
        var escenario = await CrearEscenarioAsync();

        // Actuar: se fuerza el restablecimiento (que emite un código).
        await escenario.Recuperacion.RestablecerAsync(5, 3, CancellationToken.None);

        // Verificar: el registro solo contiene el texto legible del criterio.
        AssertSinSecretos(
            Assert.Single(escenario.Unidad.Registros),
            CodigoSecreto,
            escenario.Hasher.Hash(ContrasenaNueva),
            ContrasenaNueva);
    }

    [Fact]
    public async Task RF_AUD_05_CambiarAsync_RegistroSinContrasenaHashNiCodigo()
    {
        // CA-11: el cambio de contraseña no guarda la nueva, su hash ni códigos.
        // Preparar: un usuario autenticado con contraseña actual válida.
        var escenario = await CrearEscenarioAsync();

        // Actuar: se cambia la contraseña.
        await escenario.Cambio.CambiarAsync(5, ContrasenaActual, ContrasenaNueva, CancellationToken.None);

        // Verificar: ningún campo contiene los secretos.
        AssertSinSecretos(
            Assert.Single(escenario.Unidad.Registros),
            ContrasenaNueva,
            escenario.Hasher.Hash(ContrasenaNueva),
            CodigoSecreto);
    }

    [Fact]
    public async Task RF_AUD_05_CambiarAsync_ContrasenaActualIncorrecta_NoRegistra()
    {
        // CA-13: la contraseña actual incorrecta se rechaza sin registrar ni guardar.
        // Preparar: un usuario con contraseña actual conocida.
        var escenario = await CrearEscenarioAsync();

        // Actuar: se intenta cambiar con la contraseña actual equivocada.
        await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() => escenario.Cambio.CambiarAsync(
            5, "Incorrecta!2026", ContrasenaNueva, CancellationToken.None));

        // Verificar: ni registro ni guardado, y el hash permanece intacto.
        Assert.Empty(escenario.Unidad.Registros);
        Assert.Equal(0, escenario.Unidad.Guardados);
        Assert.Equal(escenario.Hasher.Hash(ContrasenaActual), escenario.Usuario.ContraseñaHash);
    }

    [Fact]
    public async Task RF_AUD_05_ConfirmarAsync_CodigoYaUsado_NoRegistra()
    {
        // CA-13: un código ya usado se rechaza sin registrar ni guardar.
        // Preparar: un código marcado como usado.
        var escenario = await CrearEscenarioAsync();
        await SembrarCodigoAsync(escenario, usado: true);

        // Actuar: se intenta confirmar la recuperación con ese código.
        await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() => escenario.Recuperacion.ConfirmarAsync(
            CodigoSecreto, ContrasenaNueva, CancellationToken.None));

        // Verificar: ni registro ni guardado.
        Assert.Empty(escenario.Unidad.Registros);
        Assert.Equal(0, escenario.Unidad.Guardados);
    }

    [Fact]
    public async Task RF_AUD_05_RestablecerAsync_RegistroSeGuardaConElCodigo()
    {
        // CA-12: el registro se agrega dentro de EmitirCodigoAsync, antes de su único guardado.
        // Preparar: un administrador y un usuario afectado.
        var escenario = await CrearEscenarioAsync();

        // Actuar: un restablecimiento forzado correcto.
        await escenario.Recuperacion.RestablecerAsync(5, 3, CancellationToken.None);

        // Verificar: un solo guardado y el registro ya presente en él.
        Assert.Equal(1, escenario.Unidad.Guardados);
        Assert.Equal(new[] { 1 }, escenario.Unidad.RegistrosPorGuardado);
        Assert.Single(escenario.Unidad.Registros);
    }

    private static async Task<Escenario> CrearEscenarioAsync()
    {
        var reloj = new TimeProviderFijo(new DateTimeOffset(2026, 11, 21, 12, 0, 0, TimeSpan.Zero));
        var unidad = new CoreUnidadDeTrabajoFalso(reloj);
        var hasher = new HasherFalso();
        var usuario = new Usuario
        {
            Id = 5,
            Nombre = "Usuario afectado",
            Correo = "afectado@tams.test",
            ContraseñaHash = hasher.Hash(ContrasenaActual),
            Rol = RolUsuario.Estandar,
            Activo = true,
        };
        await unidad.Usuarios.AgregarAsync(usuario, CancellationToken.None);

        var registrador = new RegistradorAuditoria(unidad, reloj);
        var opciones = Options.Create(new OpcionesRecuperacionContrasena
        {
            UrlRecuperacionPlantilla = "https://ejemplo.invalid/recuperar?codigo={codigo}",
            HorasVencimientoCodigo = 1,
        });

        return new Escenario(
            unidad,
            usuario,
            hasher,
            new RecuperacionContrasenaServicio(
                unidad, registrador, new TokenGeneradorFalso(CodigoSecreto), hasher, reloj, opciones),
            new CambioContrasenaServicio(unidad, registrador, hasher));
    }

    private static async Task SembrarCodigoAsync(Escenario escenario, bool usado = false)
    {
        await escenario.Unidad.CodigosRecuperacion.AgregarAsync(new CodigoRecuperacion
        {
            UsuarioId = escenario.Usuario.Id,
            Usuario = escenario.Usuario,
            Codigo = CodigoSecreto,
            FechaEmision = new DateTime(2026, 11, 21, 12, 0, 0, DateTimeKind.Utc),
            FechaVencimiento = new DateTime(2026, 11, 21, 13, 0, 0, DateTimeKind.Utc),
            Usado = usado,
        }, CancellationToken.None);
    }

    private static void AssertSinSecretos(RegistroAuditoria registro, params string[] secretos)
    {
        // CA-11: se revisan los siete campos públicos del registro, no solo los valores.
        var campos = new[]
        {
            registro.UsuarioId.ToString(),
            registro.Accion.ToString(),
            registro.Entidad.ToString(),
            registro.EntidadId.ToString(),
            registro.FechaHora.ToString("O"),
            registro.ValorAnterior ?? string.Empty,
            registro.ValorNuevo ?? string.Empty,
        };
        var texto = string.Join("|", campos);

        foreach (var secreto in secretos)
        {
            Assert.DoesNotContain(secreto, texto, StringComparison.OrdinalIgnoreCase);
        }
    }

    private sealed record Escenario(
        CoreUnidadDeTrabajoFalso Unidad,
        Usuario Usuario,
        HasherFalso Hasher,
        RecuperacionContrasenaServicio Recuperacion,
        CambioContrasenaServicio Cambio);

    private sealed class TokenGeneradorFalso(string codigo) : ITokenGenerador
    {
        public string Generar() => codigo;
    }

    private sealed class HasherFalso : IContrasenaHasher
    {
        public string Hash(string contrasena) => $"HASH({contrasena})";

        public bool Verificar(string contrasena, string hash) => Hash(contrasena) == hash;
    }
}
