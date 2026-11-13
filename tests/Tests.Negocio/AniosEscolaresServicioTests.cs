using Core.ControlAcceso.Domain.Excepciones;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Tams.Negocio.Application.Servicios;
using Tams.Negocio.Domain.Entidades;
using Tams.Negocio.Infrastructura.Persistence;
using Xunit;

namespace Tests.Negocio;

/// <summary>
/// Pruebas del alta y la activación de años escolares (RF-NEG-02, RD-12).
/// InMemory no admite transacciones: se ignora el aviso solo en este archivo.
/// </summary>
public class AniosEscolaresServicioTests
{
    [Fact]
    public async Task Crear_ConDatosValidos_GuardaInactivoYDevuelveSusDatos()
    {
        // Preparar
        await using var contexto = CrearContexto();
        var servicio = new AniosEscolaresServicio(contexto);

        // Actuar
        var resumen = await servicio.CrearAsync(
            "  2027-2028  ",
            new DateOnly(2027, 8, 16),
            new DateOnly(2028, 6, 16));

        // Verificar
        Assert.True(resumen.Id > 0);
        Assert.Equal("2027-2028", resumen.Nombre);
        Assert.Equal(new DateOnly(2027, 8, 16), resumen.FechaInicio);
        Assert.Equal(new DateOnly(2028, 6, 16), resumen.FechaFin);
        Assert.False(resumen.EsActivo);

        var guardado = await contexto.AniosEscolares.AsNoTracking().SingleAsync();
        Assert.Equal("2027-2028", guardado.Nombre);
        Assert.False(guardado.EsActivo);
    }

    [Fact]
    public async Task Crear_ConNombreVacioOSoloEspacios_Lanza400YNoGuardaNada()
    {
        // Preparar
        await using var contexto = CrearContexto();
        var servicio = new AniosEscolaresServicio(contexto);

        // Actuar
        foreach (var nombre in new[] { "", "   ", "\t" })
        {
            var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() =>
                servicio.CrearAsync(
                    nombre,
                    new DateOnly(2027, 8, 16),
                    new DateOnly(2028, 6, 16)));

            // Verificar
            Assert.Equal(400, excepcion.CodigoHttp);
            Assert.Equal(
                "El nombre del año escolar es obligatorio y admite hasta 30 caracteres.",
                excepcion.Message);
        }

        Assert.Empty(await contexto.AniosEscolares.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task Crear_ConNombreDe31Caracteres_Lanza400()
    {
        // Preparar
        await using var contexto = CrearContexto();
        var servicio = new AniosEscolaresServicio(contexto);
        var nombre = new string('a', 31);

        // Actuar
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() =>
            servicio.CrearAsync(
                nombre,
                new DateOnly(2027, 8, 16),
                new DateOnly(2028, 6, 16)));

        // Verificar
        Assert.Equal(400, excepcion.CodigoHttp);
        Assert.Equal(
            "El nombre del año escolar es obligatorio y admite hasta 30 caracteres.",
            excepcion.Message);
        Assert.Empty(await contexto.AniosEscolares.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task Crear_ConFechaFinIgualAFechaInicio_Lanza400()
    {
        // Preparar
        await using var contexto = CrearContexto();
        var servicio = new AniosEscolaresServicio(contexto);
        var fecha = new DateOnly(2027, 8, 16);

        // Actuar
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() =>
            servicio.CrearAsync("2027-2028", fecha, fecha));

        // Verificar
        Assert.Equal(400, excepcion.CodigoHttp);
        Assert.Equal(
            "La fecha de fin debe ser posterior a la fecha de inicio.",
            excepcion.Message);
        Assert.Empty(await contexto.AniosEscolares.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task Crear_ConNombreRepetido_Lanza409YNoDuplicaElAnio()
    {
        // Preparar
        await using var contexto = CrearContexto();
        var servicio = new AniosEscolaresServicio(contexto);
        await servicio.CrearAsync(
            "2027-2028",
            new DateOnly(2027, 8, 16),
            new DateOnly(2028, 6, 16));

        // Actuar
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() =>
            servicio.CrearAsync(
                "2027-2028",
                new DateOnly(2028, 8, 16),
                new DateOnly(2029, 6, 16)));

        // Verificar
        Assert.Equal(409, excepcion.CodigoHttp);
        Assert.Equal("Ya existe un año escolar con ese nombre.", excepcion.Message);
        Assert.Equal(
            1,
            await contexto.AniosEscolares.CountAsync(a => a.Nombre == "2027-2028"));
    }

    [Fact]
    public async Task Activar_CuandoYaHayOtroActivo_DejaSoloElNuevoActivo()
    {
        // Preparar
        await using var contexto = CrearContexto();
        var anterior = await SembrarAnioAsync(contexto, "2026-2027", esActivo: true);
        var nuevo = await SembrarAnioAsync(contexto, "2027-2028", esActivo: false);
        var servicio = new AniosEscolaresServicio(contexto);

        // Actuar
        var resumen = await servicio.ActivarAsync(nuevo.Id);

        // Verificar
        Assert.Equal(nuevo.Id, resumen.Id);
        Assert.True(resumen.EsActivo);

        var anios = await contexto.AniosEscolares.AsNoTracking().ToListAsync();
        Assert.False(anios.Single(a => a.Id == anterior.Id).EsActivo);
        Assert.True(anios.Single(a => a.Id == nuevo.Id).EsActivo);
        Assert.Single(anios, a => a.EsActivo);
    }

    [Fact]
    public async Task Activar_ElAnioQueYaEstaActivo_NoCambiaNada()
    {
        // Preparar
        await using var contexto = CrearContexto();
        var activo = await SembrarAnioAsync(contexto, "2026-2027", esActivo: true);
        var servicio = new AniosEscolaresServicio(contexto);

        // Actuar
        var resumen = await servicio.ActivarAsync(activo.Id);

        // Verificar
        Assert.Equal(activo.Id, resumen.Id);
        Assert.Equal("2026-2027", resumen.Nombre);
        Assert.True(resumen.EsActivo);

        var anios = await contexto.AniosEscolares.AsNoTracking().ToListAsync();
        Assert.Single(anios, a => a.EsActivo);
        Assert.True(anios.Single(a => a.Id == activo.Id).EsActivo);
    }

    [Fact]
    public async Task Activar_UnIdInexistente_Lanza404YNoCambiaElActivo()
    {
        // Preparar
        await using var contexto = CrearContexto();
        var activo = await SembrarAnioAsync(contexto, "2026-2027", esActivo: true);
        var servicio = new AniosEscolaresServicio(contexto);

        // Actuar
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() =>
            servicio.ActivarAsync(99999));

        // Verificar
        Assert.Equal(404, excepcion.CodigoHttp);
        Assert.Equal("El año escolar no existe.", excepcion.Message);

        var anios = await contexto.AniosEscolares.AsNoTracking().ToListAsync();
        Assert.Single(anios, a => a.EsActivo);
        Assert.True(anios.Single(a => a.Id == activo.Id).EsActivo);
    }

    private static TamsDbContext CrearContexto()
    {
        var opciones = new DbContextOptionsBuilder<TamsDbContext>()
            .UseInMemoryDatabase($"AniosEscolares-{Guid.NewGuid():N}")
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new TamsDbContext(opciones);
    }

    private static async Task<AnioEscolar> SembrarAnioAsync(
        TamsDbContext contexto,
        string nombre,
        bool esActivo)
    {
        var anio = new AnioEscolar
        {
            Nombre = nombre,
            FechaInicio = new DateOnly(2026, 8, 1),
            FechaFin = new DateOnly(2027, 6, 30),
            EsActivo = esActivo,
        };

        contexto.AniosEscolares.Add(anio);
        await contexto.SaveChangesAsync();

        return anio;
    }
}
