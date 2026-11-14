using Core.ControlAcceso.Domain.Excepciones;
using Microsoft.EntityFrameworkCore;
using Tams.Negocio.Application.Modelos;
using Tams.Negocio.Application.Servicios;
using Tams.Negocio.Domain.Entidades;
using Tams.Negocio.Infrastructura.Persistence;
using Xunit;

namespace Tests.Negocio;

/// <summary>
/// Pruebas del alta y listado de cursos y estudiantes con cédula obligatoria
/// (RF-NEG-01, RF-NEG-02, RD-02, RD-07, RD-08, RD-12).
/// </summary>
public class CursosServicioTests
{
    [Fact]
    public async Task CrearCurso_NoTecnico_GuardaYDevuelveElAnioEscolar()
    {
        // Preparar
        await using var contexto = CrearContexto();
        var anio = await SembrarAnioAsync(contexto);
        var servicio = new CursosServicio(contexto);

        // Actuar
        var resumen = await servicio.CrearCursoAsync("  2do Grado  ", "  A  ", esTecnico: false, tipoTecnico: null, anio.Id);

        // Verificar
        Assert.True(resumen.Id > 0);
        Assert.Equal("2do Grado", resumen.Grado);
        Assert.Equal("A", resumen.Seccion);
        Assert.False(resumen.EsTecnico);
        Assert.Null(resumen.TipoTecnico);
        Assert.Equal(anio.Id, resumen.AnioEscolarId);
        Assert.Equal("2026-2027", resumen.AnioEscolarNombre);

        var guardado = await contexto.Cursos.AsNoTracking().SingleAsync();
        Assert.Equal("2do Grado", guardado.Grado);
        Assert.Equal("A", guardado.Seccion);
        Assert.False(guardado.EsTecnico);
        Assert.Null(guardado.TipoTecnico);
        Assert.Equal(anio.Id, guardado.AnioEscolarId);
    }

    [Fact]
    public async Task CrearCurso_TecnicoSinTipoTecnico_SeCreaConTipoNulo()
    {
        // Preparar
        await using var contexto = CrearContexto();
        var anio = await SembrarAnioAsync(contexto);
        var servicio = new CursosServicio(contexto);

        // Actuar
        var resumen = await servicio.CrearCursoAsync("1er Grado", "B", esTecnico: true, tipoTecnico: null, anio.Id);

        // Verificar
        Assert.True(resumen.EsTecnico);
        Assert.Null(resumen.TipoTecnico);

        var guardado = await contexto.Cursos.AsNoTracking().SingleAsync();
        Assert.True(guardado.EsTecnico);
        Assert.Null(guardado.TipoTecnico);
    }

    [Fact]
    public async Task CrearCurso_NoTecnicoConTipoTecnico_Lanza400YNoGuarda()
    {
        // Preparar
        await using var contexto = CrearContexto();
        var anio = await SembrarAnioAsync(contexto);
        var servicio = new CursosServicio(contexto);

        // Actuar
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() =>
            servicio.CrearCursoAsync("1er Grado", "A", esTecnico: false, tipoTecnico: "ComercioYMercadeo", anio.Id));

        // Verificar
        Assert.Equal(400, excepcion.CodigoHttp);
        Assert.Equal("Un curso no técnico no lleva tipo técnico.", excepcion.Message);
        Assert.Empty(await contexto.Cursos.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task CrearCurso_ConGradoVacio_Lanza400YNoGuarda()
    {
        // Preparar
        await using var contexto = CrearContexto();
        var anio = await SembrarAnioAsync(contexto);
        var servicio = new CursosServicio(contexto);

        // Actuar
        foreach (var grado in new[] { "", "   ", new string('a', 31) })
        {
            var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() =>
                servicio.CrearCursoAsync(grado, "A", esTecnico: false, tipoTecnico: null, anio.Id));

            // Verificar
            Assert.Equal(400, excepcion.CodigoHttp);
            Assert.Equal(
                "El grado es obligatorio y admite hasta 30 caracteres.",
                excepcion.Message);
        }

        Assert.Empty(await contexto.Cursos.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task CrearCurso_ConAnioInexistente_Lanza404YNoGuarda()
    {
        // Preparar
        await using var contexto = CrearContexto();
        await SembrarAnioAsync(contexto);
        var servicio = new CursosServicio(contexto);

        // Actuar
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() =>
            servicio.CrearCursoAsync("1er Grado", "A", esTecnico: false, tipoTecnico: null, anioEscolarId: 99999));

        // Verificar
        Assert.Equal(404, excepcion.CodigoHttp);
        Assert.Equal("El año escolar no existe.", excepcion.Message);
        Assert.Empty(await contexto.Cursos.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task CrearCurso_RepetidoEnOtrasMayusculas_Lanza409YNoDuplica()
    {
        // Preparar
        await using var contexto = CrearContexto();
        var anio = await SembrarAnioAsync(contexto);
        var servicio = new CursosServicio(contexto);
        await servicio.CrearCursoAsync("1er Grado", "A", esTecnico: false, tipoTecnico: null, anio.Id);

        // Actuar
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() =>
            servicio.CrearCursoAsync("1ER GRADO", "a", esTecnico: false, tipoTecnico: null, anio.Id));

        // Verificar
        Assert.Equal(409, excepcion.CodigoHttp);
        Assert.Equal(
            "Ya existe un curso con ese grado y sección en ese año escolar.",
            excepcion.Message);
        Assert.Equal(1, await contexto.Cursos.CountAsync());
    }

    [Fact]
    public async Task ListarCursos_ConFiltroDeAnio_DevuelveSoloEseAnioOrdenado()
    {
        // Preparar
        await using var contexto = CrearContexto();
        var anioActual = await SembrarAnioAsync(contexto, "2026-2027");
        var anioAnterior = await SembrarAnioAsync(contexto, "2025-2026");
        await SembrarCursoAsync(contexto, anioActual.Id, "2do Grado", "B");
        await SembrarCursoAsync(contexto, anioActual.Id, "1er Grado", "A");
        await SembrarCursoAsync(contexto, anioAnterior.Id, "3er Grado", "A");
        var servicio = new CursosServicio(contexto);

        // Actuar
        var cursos = await servicio.ListarCursosAsync(anioActual.Id);

        // Verificar
        Assert.Equal(2, cursos.Count);
        Assert.All(cursos, curso => Assert.Equal(anioActual.Id, curso.AnioEscolarId));
        Assert.All(cursos, curso => Assert.Equal("2026-2027", curso.AnioEscolarNombre));
        Assert.Equal(new[] { "1er Grado", "2do Grado" }, cursos.Select(curso => curso.Grado));
    }

    [Fact]
    public async Task CrearEstudiante_ConCedulaConGuiones_GuardaOnceDigitos()
    {
        // Preparar
        await using var contexto = CrearContexto();
        var anio = await SembrarAnioAsync(contexto);
        var curso = await SembrarCursoAsync(contexto, anio.Id, "1er Grado", "A");
        var servicio = new CursosServicio(contexto);

        // Actuar
        var resumen = await servicio.CrearEstudianteAsync(
            curso.Id,
            "  María ",
            " Rodríguez ",
            "001-2345678-9");

        // Verificar
        Assert.True(resumen.Id > 0);
        Assert.Equal("María", resumen.Nombre);
        Assert.Equal("Rodríguez", resumen.Apellido);
        Assert.Equal("00123456789", resumen.Cedula);
        Assert.Equal(curso.Id, resumen.CursoId);

        var guardado = await contexto.Estudiantes.AsNoTracking().SingleAsync();
        Assert.Equal("00123456789", guardado.Cedula);
        Assert.Equal(curso.Id, guardado.CursoId);
    }

    [Fact]
    public async Task CrearEstudiante_ConCedulaInvalida_Lanza400YNoGuarda()
    {
        // Preparar
        await using var contexto = CrearContexto();
        var anio = await SembrarAnioAsync(contexto);
        var curso = await SembrarCursoAsync(contexto, anio.Id, "1er Grado", "A");
        var servicio = new CursosServicio(contexto);

        // Actuar
        foreach (var cedula in new[] { "0012345678", "0012345678A" })
        {
            var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() =>
                servicio.CrearEstudianteAsync(curso.Id, "María", "Rodríguez", cedula));

            // Verificar
            Assert.Equal(400, excepcion.CodigoHttp);
            Assert.Equal(
                "La cédula es obligatoria y debe tener 11 dígitos (con o sin guiones).",
                excepcion.Message);
        }

        Assert.Empty(await contexto.Estudiantes.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task CrearEstudiante_SinCedula_Lanza400YNoGuarda()
    {
        // Preparar
        await using var contexto = CrearContexto();
        var anio = await SembrarAnioAsync(contexto);
        var curso = await SembrarCursoAsync(contexto, anio.Id, "1er Grado", "A");
        var servicio = new CursosServicio(contexto);

        // Actuar
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() =>
            servicio.CrearEstudianteAsync(curso.Id, "María", "Rodríguez", cedula: null));

        // Verificar
        Assert.Equal(400, excepcion.CodigoHttp);
        Assert.Equal(
            "La cédula es obligatoria y debe tener 11 dígitos (con o sin guiones).",
            excepcion.Message);
        Assert.Empty(await contexto.Estudiantes.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task CrearEstudiante_ConCedulaRepetidaEnOtroCurso_Lanza409YNoGuarda()
    {
        // Preparar
        await using var contexto = CrearContexto();
        var anio = await SembrarAnioAsync(contexto);
        var curso1 = await SembrarCursoAsync(contexto, anio.Id, "1er Grado", "A");
        var curso2 = await SembrarCursoAsync(contexto, anio.Id, "1er Grado", "B");
        var servicio = new CursosServicio(contexto);
        await servicio.CrearEstudianteAsync(curso1.Id, "Ana", "Pérez", "001-2345678-9");

        // Actuar
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() =>
            servicio.CrearEstudianteAsync(curso2.Id, "Luis", "Gómez", "00123456789"));

        // Verificar
        Assert.Equal(409, excepcion.CodigoHttp);
        Assert.Equal("Ya existe un estudiante con esa cédula.", excepcion.Message);
        Assert.Equal(1, await contexto.Estudiantes.CountAsync());
        Assert.Equal(curso1.Id, (await contexto.Estudiantes.AsNoTracking().SingleAsync()).CursoId);
    }

    [Fact]
    public async Task CrearEstudiante_EnCursoInexistente_Lanza404YNoGuarda()
    {
        // Preparar
        await using var contexto = CrearContexto();
        await SembrarAnioAsync(contexto);
        var servicio = new CursosServicio(contexto);

        // Actuar
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() =>
            servicio.CrearEstudianteAsync(99999, "María", "Rodríguez", "00123456789"));

        // Verificar
        Assert.Equal(404, excepcion.CodigoHttp);
        Assert.Equal("El curso no existe.", excepcion.Message);
        Assert.Empty(await contexto.Estudiantes.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task ListarEstudiantes_DevuelveOrdenadosPorApellidoYNombreConCedula()
    {
        // Preparar
        await using var contexto = CrearContexto();
        var anio = await SembrarAnioAsync(contexto);
        var curso = await SembrarCursoAsync(contexto, anio.Id, "1er Grado", "A");
        contexto.Estudiantes.AddRange(
            new Estudiante
            {
                Nombre = "Ana",
                Apellido = "Pérez",
                Cedula = "00000000001",
                CursoId = curso.Id,
            },
            new Estudiante
            {
                Nombre = "Luis",
                Apellido = "Gómez",
                Cedula = "00000000002",
                CursoId = curso.Id,
            },
            new Estudiante
            {
                Nombre = "Ana",
                Apellido = "Gómez",
                Cedula = "00000000003",
                CursoId = curso.Id,
            });
        await contexto.SaveChangesAsync();
        var servicio = new CursosServicio(contexto);

        // Actuar
        var estudiantes = await servicio.ListarEstudiantesAsync(curso.Id);

        // Verificar
        Assert.Equal(3, estudiantes.Count);
        Assert.Equal(
            new[] { "Gómez Ana", "Gómez Luis", "Pérez Ana" },
            estudiantes.Select(estudiante => $"{estudiante.Apellido} {estudiante.Nombre}"));
        Assert.Equal(
            new[] { "00000000003", "00000000002", "00000000001" },
            estudiantes.Select(estudiante => estudiante.Cedula));
        Assert.All(estudiantes, estudiante => Assert.Equal(curso.Id, estudiante.CursoId));
    }

    [Fact]
    public void EstudianteResumen_DelDocente_NoExponeLaCedula()
    {
        // Actuar
        var propiedades = typeof(EstudianteResumen).GetProperties();

        // Verificar
        Assert.DoesNotContain(
            propiedades,
            propiedad => propiedad.Name.Equals("Cedula", StringComparison.OrdinalIgnoreCase));
    }

    private static TamsDbContext CrearContexto()
    {
        var opciones = new DbContextOptionsBuilder<TamsDbContext>()
            .UseInMemoryDatabase($"Cursos-{Guid.NewGuid():N}")
            .Options;

        return new TamsDbContext(opciones);
    }

    private static async Task<AnioEscolar> SembrarAnioAsync(
        TamsDbContext contexto,
        string nombre = "2026-2027")
    {
        var anio = new AnioEscolar
        {
            Nombre = nombre,
            FechaInicio = new DateOnly(2026, 8, 1),
            FechaFin = new DateOnly(2027, 6, 30),
            EsActivo = nombre == "2026-2027",
        };

        contexto.AniosEscolares.Add(anio);
        await contexto.SaveChangesAsync();

        return anio;
    }

    private static async Task<Curso> SembrarCursoAsync(
        TamsDbContext contexto,
        int anioEscolarId,
        string grado,
        string seccion)
    {
        var curso = new Curso
        {
            Grado = grado,
            Seccion = seccion,
            EsTecnico = false,
            AnioEscolarId = anioEscolarId,
        };

        contexto.Cursos.Add(curso);
        await contexto.SaveChangesAsync();

        return curso;
    }
}
