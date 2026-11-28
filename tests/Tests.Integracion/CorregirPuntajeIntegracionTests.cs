using Core.ControlAcceso.Domain.Excepciones;
using Microsoft.EntityFrameworkCore;
using Tams.Negocio.Application;
using Tams.Negocio.Domain.Entidades;
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

    [Fact]
    public async Task RF_NEG_CorregirRaPublicadoPorDebajoDelMinimo_SeGuardaConAdvertencia()
    {
        // Preparar: permiso aprobado como en la corrección existente y RA1 con mínimo 70.
        await using var contexto = await ContextoIntegracion.CrearAsync();
        var solicitud = await contexto.GestionPermisos.CrearAsync(
            ContextoIntegracion.DocenteId,
            TiposPermiso.CorregirCalificacion,
            CancellationToken.None);
        await contexto.GestionPermisos.AprobarAsync(
            solicitud.Id,
            ContextoIntegracion.AdministradorId,
            CancellationToken.None);

        var calificacion = await contexto.TamsContext.Calificaciones
            .SingleAsync(c => c.Id == ContextoIntegracion.CalificacionId);
        calificacion.TipoEvaluacion = TipoEvaluacion.RA;
        calificacion.Periodo = null;
        calificacion.NumeroRA = 1;

        var materia = await contexto.TamsContext.Materias
            .SingleAsync(m => m.Id == calificacion.MateriaId);
        materia.EsTecnica = true;
        materia.CantidadRA = 1;
        contexto.TamsContext.ResultadosAprendizaje.Add(new ResultadoAprendizaje
        {
            MateriaId = materia.Id,
            Numero = 1,
            Puntos = 100,
            PuntajeMinimo = 70,
        });
        await contexto.TamsContext.SaveChangesAsync();

        // Actuar: se corrige el RA1 publicado a 60.
        var resultado = await contexto.Calificaciones.CorregirPuntajeAsync(
            ContextoIntegracion.DocenteId,
            ContextoIntegracion.CalificacionId,
            60m,
            CancellationToken.None);

        // Verificar: se guarda, sigue Publicada y solo advierte.
        Assert.Equal(60m, resultado.PuntajeObtenido);
        Assert.Equal(60m, await contexto.ObtenerPuntajeAsync());
        Assert.Equal("Publicada", resultado.Estado);
        Assert.Equal(EstadoCalificacion.Publicada, await contexto.ObtenerEstadoAsync());
        Assert.Equal("La nota del RA1 está por debajo del mínimo de 70 puntos.", resultado.Advertencia);
    }
}
