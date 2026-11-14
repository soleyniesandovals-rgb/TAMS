using Core.ControlAcceso.Domain.Excepciones;
using Microsoft.EntityFrameworkCore;
using Tams.Negocio.Application.Modelos;
using Tams.Negocio.Application.Servicios;
using Tams.Negocio.Domain.Enums;
using Tams.Negocio.Infrastructura.Persistence;
using Xunit;

namespace Tests.Negocio;

/// <summary>
/// Pruebas del alta y listado de materias con sus resultados de aprendizaje
/// (RF-NEG-01, RF-NEG-02, RD-12).
/// </summary>
public class MateriasServicioTests
{
    [Fact]
    public async Task Crear_NoTecnica_GuardaSinTipoSinCantidadYSinRa()
    {
        // Preparar
        await using var contexto = CrearContexto();
        var servicio = new MateriasServicio(contexto);

        // Actuar
        var resumen = await servicio.CrearAsync("  Matemática  ", esTecnica: false, tipoTecnico: null, resultadosAprendizaje: null);

        // Verificar
        Assert.True(resumen.Id > 0);
        Assert.Equal("Matemática", resumen.Nombre);
        Assert.False(resumen.EsTecnica);
        Assert.Null(resumen.TipoTecnico);
        Assert.Null(resumen.CantidadRa);
        Assert.Empty(resumen.ResultadosAprendizaje);

        var guardada = await contexto.Materias.AsNoTracking().SingleAsync();
        Assert.Equal("Matemática", guardada.Nombre);
        Assert.Null(guardada.TipoTecnico);
        Assert.Null(guardada.CantidadRA);
        Assert.Empty(await contexto.ResultadosAprendizaje.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task Crear_TecnicaConTresRa_GuardaCantidadYRaOrdenados()
    {
        // Preparar
        await using var contexto = CrearContexto();
        var servicio = new MateriasServicio(contexto);

        // Actuar
        var resumen = await servicio.CrearAsync(
            "Programación",
            esTecnica: true,
            tipoTecnico: "DesarrolloAdministracionAplicacionesInformaticas",
            resultadosAprendizaje:
            [
                Ra(2, 30, "RA 2"),
                Ra(1, 40, "RA 1"),
                Ra(3, 30, "RA 3"),
            ]);

        // Verificar
        Assert.Equal(TipoTecnico.DesarrolloAdministracionAplicacionesInformaticas, resumen.TipoTecnico);
        Assert.Equal(3, resumen.CantidadRa);
        Assert.Equal(new[] { 1, 2, 3 }, resumen.ResultadosAprendizaje.Select(r => r.Numero));
        Assert.Equal(new[] { 40, 30, 30 }, resumen.ResultadosAprendizaje.Select(r => r.Puntos));

        var guardados = await contexto.ResultadosAprendizaje
            .AsNoTracking()
            .OrderBy(r => r.Numero)
            .ToListAsync();

        Assert.Equal(3, guardados.Count);
        Assert.Equal(new[] { 1, 2, 3 }, guardados.Select(r => r.Numero));
        Assert.Equal(new[] { 40, 30, 30 }, guardados.Select(r => r.Puntos));
        Assert.Equal(100, guardados.Sum(r => r.Puntos));
        Assert.All(guardados, r => Assert.NotEqual(default, r.FechaCreacion));
    }

    [Fact]
    public async Task Crear_ConNombreVacio_Lanza400YNoGuardaNada()
    {
        // Preparar
        await using var contexto = CrearContexto();
        var servicio = new MateriasServicio(contexto);

        // Actuar
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() =>
            servicio.CrearAsync("", esTecnica: false, tipoTecnico: null, resultadosAprendizaje: null));

        // Verificar
        Assert.Equal(400, excepcion.CodigoHttp);
        Assert.Equal(
            "El nombre de la materia es obligatorio y admite hasta 150 caracteres.",
            excepcion.Message);
        await AssertSinDatosAsync(contexto);
    }

    [Fact]
    public async Task Crear_ConNombreRepetidoEnOtrasMayusculas_Lanza409YNoDuplica()
    {
        // Preparar
        await using var contexto = CrearContexto();
        var servicio = new MateriasServicio(contexto);
        await servicio.CrearAsync("Matemática", esTecnica: false, tipoTecnico: null, resultadosAprendizaje: null);

        // Actuar
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() =>
            servicio.CrearAsync("matemática", esTecnica: false, tipoTecnico: null, resultadosAprendizaje: null));

        // Verificar
        Assert.Equal(409, excepcion.CodigoHttp);
        Assert.Equal("Ya existe una materia con ese nombre.", excepcion.Message);
        Assert.Equal(1, await contexto.Materias.CountAsync());
        Assert.Equal(1, await contexto.Materias.CountAsync(m => m.Nombre == "Matemática"));
        Assert.Empty(await contexto.ResultadosAprendizaje.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task Crear_NoTecnicaConTipoTecnico_Lanza400YNoGuardaNada()
    {
        // Preparar
        await using var contexto = CrearContexto();
        var servicio = new MateriasServicio(contexto);

        // Actuar
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() =>
            servicio.CrearAsync(
                "Matemática",
                esTecnica: false,
                tipoTecnico: "DesarrolloAdministracionAplicacionesInformaticas",
                resultadosAprendizaje: null));

        // Verificar
        Assert.Equal(400, excepcion.CodigoHttp);
        Assert.Equal(
            "Una materia no técnica no lleva tipo técnico ni resultados de aprendizaje.",
            excepcion.Message);
        await AssertSinDatosAsync(contexto);
    }

    [Fact]
    public async Task Crear_NoTecnicaConUnRa_Lanza400YNoGuardaNada()
    {
        // Preparar
        await using var contexto = CrearContexto();
        var servicio = new MateriasServicio(contexto);

        // Actuar
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() =>
            servicio.CrearAsync(
                "Matemática",
                esTecnica: false,
                tipoTecnico: null,
                resultadosAprendizaje: [Ra(1, 100, "RA 1")]));

        // Verificar
        Assert.Equal(400, excepcion.CodigoHttp);
        Assert.Equal(
            "Una materia no técnica no lleva tipo técnico ni resultados de aprendizaje.",
            excepcion.Message);
        await AssertSinDatosAsync(contexto);
    }

    [Fact]
    public async Task Crear_TecnicaSinTipoTecnico_Lanza400YNoGuardaNada()
    {
        // Preparar
        await using var contexto = CrearContexto();
        var servicio = new MateriasServicio(contexto);

        // Actuar
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() =>
            servicio.CrearAsync(
                "Programación",
                esTecnica: true,
                tipoTecnico: null,
                resultadosAprendizaje: [Ra(1, 100, "RA 1")]));

        // Verificar
        Assert.Equal(400, excepcion.CodigoHttp);
        Assert.Equal(
            "El tipo técnico es obligatorio en una materia técnica y debe ser ComercioYMercadeo o DesarrolloAdministracionAplicacionesInformaticas.",
            excepcion.Message);
        await AssertSinDatosAsync(contexto);
    }

    [Fact]
    public async Task Crear_TecnicaConTipoTecnicoInexistente_Lanza400YNoGuardaNada()
    {
        // Preparar
        await using var contexto = CrearContexto();
        var servicio = new MateriasServicio(contexto);

        // Actuar
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() =>
            servicio.CrearAsync(
                "Programación",
                esTecnica: true,
                tipoTecnico: "Cocina",
                resultadosAprendizaje: [Ra(1, 100, "RA 1")]));

        // Verificar
        Assert.Equal(400, excepcion.CodigoHttp);
        Assert.Equal(
            "El tipo técnico es obligatorio en una materia técnica y debe ser ComercioYMercadeo o DesarrolloAdministracionAplicacionesInformaticas.",
            excepcion.Message);
        await AssertSinDatosAsync(contexto);
    }

    [Fact]
    public async Task Crear_TecnicaSinRa_Lanza400YNoGuardaNada()
    {
        // Preparar
        await using var contexto = CrearContexto();
        var servicio = new MateriasServicio(contexto);

        // Actuar
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() =>
            servicio.CrearAsync(
                "Programación",
                esTecnica: true,
                tipoTecnico: "ComercioYMercadeo",
                resultadosAprendizaje: null));

        // Verificar
        Assert.Equal(400, excepcion.CodigoHttp);
        Assert.Equal(
            "Una materia técnica necesita al menos un resultado de aprendizaje.",
            excepcion.Message);
        await AssertSinDatosAsync(contexto);
    }

    [Fact]
    public async Task Crear_TecnicaConNumeros1Y3_Lanza400YNoGuardaNada()
    {
        // Preparar
        await using var contexto = CrearContexto();
        var servicio = new MateriasServicio(contexto);

        // Actuar
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() =>
            servicio.CrearAsync(
                "Programación",
                esTecnica: true,
                tipoTecnico: "ComercioYMercadeo",
                resultadosAprendizaje: [Ra(1, 50, "RA 1"), Ra(3, 50, "RA 3")]));

        // Verificar
        Assert.Equal(400, excepcion.CodigoHttp);
        Assert.Equal(
            "Los resultados de aprendizaje deben numerarse 1, 2, 3… sin repetir ni saltar números.",
            excepcion.Message);
        await AssertSinDatosAsync(contexto);
    }

    [Fact]
    public async Task Crear_TecnicaConRaDeCeroPuntos_Lanza400YNoGuardaNada()
    {
        // Preparar
        await using var contexto = CrearContexto();
        var servicio = new MateriasServicio(contexto);

        // Actuar
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() =>
            servicio.CrearAsync(
                "Programación",
                esTecnica: true,
                tipoTecnico: "ComercioYMercadeo",
                resultadosAprendizaje: [Ra(1, 0, "RA 1"), Ra(2, 100, "RA 2")]));

        // Verificar
        Assert.Equal(400, excepcion.CodigoHttp);
        Assert.Equal("Cada resultado de aprendizaje vale entre 1 y 100 puntos.", excepcion.Message);
        await AssertSinDatosAsync(contexto);
    }

    [Fact]
    public async Task Crear_TecnicaCuyosPuntosSuman90_Lanza400YNoGuardaNada()
    {
        // Preparar
        await using var contexto = CrearContexto();
        var servicio = new MateriasServicio(contexto);

        // Actuar
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() =>
            servicio.CrearAsync(
                "Programación",
                esTecnica: true,
                tipoTecnico: "ComercioYMercadeo",
                resultadosAprendizaje: [Ra(1, 45, "RA 1"), Ra(2, 45, "RA 2")]));

        // Verificar
        Assert.Equal(400, excepcion.CodigoHttp);
        Assert.Contains("suman 90", excepcion.Message);
        await AssertSinDatosAsync(contexto);
    }

    [Fact]
    public async Task Listar_DevuelveMateriasPorNombreYRaPorNumero()
    {
        // Preparar
        await using var contexto = CrearContexto();
        var servicio = new MateriasServicio(contexto);
        await servicio.CrearAsync("Zeta", esTecnica: false, tipoTecnico: null, resultadosAprendizaje: null);
        await servicio.CrearAsync(
            "Alfa",
            esTecnica: true,
            tipoTecnico: "ComercioYMercadeo",
            resultadosAprendizaje:
            [
                Ra(3, 30, "RA 3"),
                Ra(1, 40, "RA 1"),
                Ra(2, 30, "RA 2"),
            ]);

        // Actuar
        var materias = await servicio.ListarAsync();

        // Verificar
        Assert.Equal(new[] { "Alfa", "Zeta" }, materias.Select(m => m.Nombre));
        Assert.Equal(new[] { 1, 2, 3 }, materias[0].ResultadosAprendizaje.Select(r => r.Numero));
        Assert.Empty(materias[1].ResultadosAprendizaje);
    }

    private static TamsDbContext CrearContexto()
    {
        var opciones = new DbContextOptionsBuilder<TamsDbContext>()
            .UseInMemoryDatabase($"Materias-{Guid.NewGuid():N}")
            .Options;

        return new TamsDbContext(opciones);
    }

    private static ResultadoAprendizajeEntrada Ra(int numero, int puntos, string? descripcion = null)
        => new(numero, descripcion, puntos);

    private static async Task AssertSinDatosAsync(TamsDbContext contexto)
    {
        Assert.Empty(await contexto.Materias.AsNoTracking().ToListAsync());
        Assert.Empty(await contexto.ResultadosAprendizaje.AsNoTracking().ToListAsync());
    }
}
