using Core.Auditoria.Application.Servicios;
using Core.Auditoria.Domain.Enums;
using Core.ControlAcceso.Application.Servicios;
using Core.ControlAcceso.Domain.Entidades;
using Core.ControlAcceso.Domain.Enums;
using Core.ControlAcceso.Domain.Excepciones;
using Tests.Core.Dobles;
using Xunit;

namespace Tests.Core;

/// <summary>
/// RF-AUD-04: auditoría del cambio de rol, la desactivación y la reactivación
/// (CA-06, CA-07, CA-12 y CA-13), con el registrador real sobre una unidad falsa.
/// </summary>
public class AdministracionCuentasAuditoriaTests
{
    [Fact]
    public async Task RF_AUD_04_CambiarRolAsync_RegistraLosSeisCampos()
    {
        // CA-06: CambiarRol / Usuario / id del afectado / rol anterior -> rol nuevo.
        // Preparar: un usuario Estándar y un administrador distinto.
        var (servicio, unidad) = await CrearAsync(
            UsuarioDePrueba(5, RolUsuario.Estandar),
            UsuarioDePrueba(3, RolUsuario.Administrador));

        // Actuar: el administrador cambia el rol del usuario 5.
        await servicio.CambiarRolAsync(5, "Administrador", 3, CancellationToken.None);

        // Verificar: un registro con los seis campos y el rol anterior correcto.
        var registro = Assert.Single(unidad.Registros);
        Assert.Equal(3, registro.UsuarioId);
        Assert.Equal(AccionAuditoria.CambiarRol, registro.Accion);
        Assert.Equal(EntidadAuditoria.Usuario, registro.Entidad);
        Assert.Equal(5, registro.EntidadId);
        Assert.Equal("Estandar", registro.ValorAnterior);
        Assert.Equal("Administrador", registro.ValorNuevo);
    }

    [Fact]
    public async Task RF_AUD_04_CambiarRolAsync_MismoRol_NoRegistra()
    {
        // CA-06: si el usuario ya tenía ese rol, la operación no cambia nada y no se registra.
        // Preparar: un usuario Estándar.
        var (servicio, unidad) = await CrearAsync(
            UsuarioDePrueba(5, RolUsuario.Estandar),
            UsuarioDePrueba(3, RolUsuario.Administrador));

        // Actuar: se solicita el mismo rol, con distinta capitalización.
        await servicio.CambiarRolAsync(5, "estandar", 3, CancellationToken.None);

        // Verificar: ni registro ni guardado.
        Assert.Empty(unidad.Registros);
        Assert.Equal(0, unidad.Guardados);
    }

    [Fact]
    public async Task RF_AUD_04_DesactivarAsync_RegistraActivoAInactivo()
    {
        // CA-07: DesactivarUsuario con el administrador del token como actor.
        // Preparar: un usuario activo y un administrador.
        var (servicio, unidad) = await CrearAsync(
            UsuarioDePrueba(5, RolUsuario.Estandar),
            UsuarioDePrueba(3, RolUsuario.Administrador));

        // Actuar: el administrador desactiva al usuario.
        await servicio.DesactivarAsync(5, 3, CancellationToken.None);

        // Verificar: los seis campos, con Activo -> Inactivo.
        var registro = Assert.Single(unidad.Registros);
        Assert.Equal(3, registro.UsuarioId);
        Assert.Equal(AccionAuditoria.DesactivarUsuario, registro.Accion);
        Assert.Equal(EntidadAuditoria.Usuario, registro.Entidad);
        Assert.Equal(5, registro.EntidadId);
        Assert.Equal("Activo", registro.ValorAnterior);
        Assert.Equal("Inactivo", registro.ValorNuevo);
    }

    [Fact]
    public async Task RF_AUD_04_ReactivarAsync_RegistraInactivoAActivo()
    {
        // CA-07: ReactivarUsuario con el administrador recibido como parámetro nuevo.
        // Preparar: un usuario inactivo y un administrador.
        var (servicio, unidad) = await CrearAsync(
            UsuarioDePrueba(5, RolUsuario.Estandar, activo: false),
            UsuarioDePrueba(3, RolUsuario.Administrador));

        // Actuar: el administrador reactiva al usuario.
        await servicio.ReactivarAsync(5, 3, CancellationToken.None);

        // Verificar: los seis campos, con el administrador como actor.
        var registro = Assert.Single(unidad.Registros);
        Assert.Equal(3, registro.UsuarioId);
        Assert.Equal(AccionAuditoria.ReactivarUsuario, registro.Accion);
        Assert.Equal(EntidadAuditoria.Usuario, registro.Entidad);
        Assert.Equal(5, registro.EntidadId);
        Assert.Equal("Inactivo", registro.ValorAnterior);
        Assert.Equal("Activo", registro.ValorNuevo);
    }

    [Fact]
    public async Task RF_AUD_04_CambiarRolAsync_RegistroSeGuardaConLaOperacion()
    {
        // CA-12: el registro se agrega antes del único guardado de la operación.
        // Preparar: un usuario Estándar y un administrador.
        var (servicio, unidad) = await CrearAsync(
            UsuarioDePrueba(5, RolUsuario.Estandar),
            UsuarioDePrueba(3, RolUsuario.Administrador));

        // Actuar: un cambio de rol correcto.
        await servicio.CambiarRolAsync(5, "Administrador", 3, CancellationToken.None);

        // Verificar: un solo guardado y el registro ya estaba presente en él.
        Assert.Equal(1, unidad.Guardados);
        Assert.Equal(new[] { 1 }, unidad.RegistrosPorGuardado);
        Assert.Single(unidad.Registros);
    }

    [Fact]
    public async Task RF_AUD_04_CambiarRolAsync_PropioRolRechazado_NoRegistra()
    {
        // CA-13: una operación rechazada con 409 no genera ningún registro.
        // Preparar: solo el administrador, que intenta cambiar su propio rol.
        var (servicio, unidad) = await CrearAsync(UsuarioDePrueba(3, RolUsuario.Administrador));

        // Actuar: intenta cambiar su propio rol.
        await Assert.ThrowsAsync<ReglaNegocioExcepcion>(
            () => servicio.CambiarRolAsync(3, "Estandar", 3, CancellationToken.None));

        // Verificar: ni registro ni guardado.
        Assert.Empty(unidad.Registros);
        Assert.Equal(0, unidad.Guardados);
    }

    private static async Task<(AdministracionCuentasServicio Servicio, CoreUnidadDeTrabajoFalso Unidad)> CrearAsync(
        params Usuario[] usuarios)
    {
        var reloj = new TimeProviderFijo(new DateTimeOffset(2026, 11, 21, 12, 0, 0, TimeSpan.Zero));
        var unidad = new CoreUnidadDeTrabajoFalso(reloj);

        foreach (var usuario in usuarios)
        {
            await unidad.Usuarios.AgregarAsync(usuario, CancellationToken.None);
        }

        var registrador = new RegistradorAuditoria(unidad, reloj);
        return (new AdministracionCuentasServicio(unidad, registrador), unidad);
    }

    private static Usuario UsuarioDePrueba(int id, RolUsuario rol, bool activo = true)
        => new()
        {
            Id = id,
            Nombre = $"Usuario {id}",
            Correo = $"usuario{id}@tams.test",
            ContraseñaHash = "hash-de-prueba",
            Rol = rol,
            Activo = activo,
        };
}
