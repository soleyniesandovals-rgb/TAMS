using Core.ControlAcceso.Domain.Excepciones;
using Tams.Negocio.Application;
using Tams.Negocio.Domain.Enums;
using Xunit;

namespace Tests.Integracion;

/// <summary>
/// RF-NEG-06: la corrección de una calificación publicada usa la Gestión de permisos real
/// y el módulo de negocio real, sin dobles y sin levantar la aplicación.
/// </summary>
public class CorregirPuntajeIntegracionTests
{
    [Fact]
    public async Task Corregir_sinSolicitudDePermiso_seRechazaYElPuntajeNoCambia()
    {
        // Preparar
        await using var contexto = await ContextoIntegracion.CrearAsync();
        const decimal nuevoPuntaje = 90m;

        // Actuar
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() =>
            contexto.Calificaciones.CorregirPuntajeAsync(
                ContextoIntegracion.DocenteId,
                ContextoIntegracion.CalificacionId,
                nuevoPuntaje,
                CancellationToken.None));

        // Verificar
        Assert.Equal(403, excepcion.CodigoHttp);
        Assert.Equal(ContextoIntegracion.PuntajeInicial, await contexto.ObtenerPuntajeAsync());
    }

    [Fact]
    public async Task Corregir_conSolicitudPendiente_seRechazaYElPuntajeNoCambia()
    {
        // Preparar
        await using var contexto = await ContextoIntegracion.CrearAsync();
        await contexto.GestionPermisos.CrearAsync(
            ContextoIntegracion.DocenteId,
            TiposPermiso.CorregirCalificacion,
            CancellationToken.None);
        const decimal nuevoPuntaje = 90m;

        // Actuar
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() =>
            contexto.Calificaciones.CorregirPuntajeAsync(
                ContextoIntegracion.DocenteId,
                ContextoIntegracion.CalificacionId,
                nuevoPuntaje,
                CancellationToken.None));

        // Verificar
        Assert.Equal(403, excepcion.CodigoHttp);
        Assert.Equal(ContextoIntegracion.PuntajeInicial, await contexto.ObtenerPuntajeAsync());
    }

    [Fact]
    public async Task Corregir_conSolicitudRechazada_seRechazaYElPuntajeNoCambia()
    {
        // Preparar
        await using var contexto = await ContextoIntegracion.CrearAsync();
        var solicitud = await contexto.GestionPermisos.CrearAsync(
            ContextoIntegracion.DocenteId,
            TiposPermiso.CorregirCalificacion,
            CancellationToken.None);
        await contexto.GestionPermisos.RechazarAsync(
            solicitud.Id,
            ContextoIntegracion.AdministradorId,
            "El permiso no procede.",
            CancellationToken.None);
        const decimal nuevoPuntaje = 90m;

        // Actuar
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() =>
            contexto.Calificaciones.CorregirPuntajeAsync(
                ContextoIntegracion.DocenteId,
                ContextoIntegracion.CalificacionId,
                nuevoPuntaje,
                CancellationToken.None));

        // Verificar
        Assert.Equal(403, excepcion.CodigoHttp);
        Assert.Equal(ContextoIntegracion.PuntajeInicial, await contexto.ObtenerPuntajeAsync());
    }

    [Fact]
    public async Task Corregir_trasAprobarElPermiso_seEjecutaYElEstadoNoCambia()
    {
        // Preparar
        await using var contexto = await ContextoIntegracion.CrearAsync();
        var solicitud = await contexto.GestionPermisos.CrearAsync(
            ContextoIntegracion.DocenteId,
            TiposPermiso.CorregirCalificacion,
            CancellationToken.None);
        await contexto.GestionPermisos.AprobarAsync(
            solicitud.Id,
            ContextoIntegracion.AdministradorId,
            CancellationToken.None);
        const decimal nuevoPuntaje = 90m;

        // Actuar
        var resultado = await contexto.Calificaciones.CorregirPuntajeAsync(
            ContextoIntegracion.DocenteId,
            ContextoIntegracion.CalificacionId,
            nuevoPuntaje,
            CancellationToken.None);

        // Verificar
        Assert.Equal(nuevoPuntaje, resultado.PuntajeObtenido);
        Assert.Equal("Publicada", resultado.Estado);
        Assert.Equal(EstadoCalificacion.Publicada, await contexto.ObtenerEstadoAsync());
        Assert.True(await contexto.GestionPermisos.TienePermisoAsync(
            ContextoIntegracion.DocenteId,
            TiposPermiso.CorregirCalificacion,
            CancellationToken.None));
    }
}
