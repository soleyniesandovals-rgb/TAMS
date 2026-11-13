using Core.ControlAcceso.Domain.Excepciones;
using Microsoft.EntityFrameworkCore;
using Tams.Negocio.Application.Servicios;
using Tams.Negocio.Domain.Entidades;
using Tams.Negocio.Infrastructura.Persistence;
using Xunit;

namespace Tests.Negocio;

/// <summary>
/// Pruebas de las consultas de solo lectura del docente (RF-NEG-02, RD-12).
/// </summary>
public class ConsultasDocenteServicioTests
{
    [Fact]
    public async Task MisAsignaciones_DevuelveSoloLasDelDocenteConNombresDeMateriaYCurso()
    {
        // Preparar
        await using var contexto = CrearContexto();
        await SembrarEscenarioAsync(contexto);
        var servicio = new ConsultasDocenteServicio(contexto);

        // Actuar
        var asignaciones = await servicio.ListarMisAsignacionesAsync(10);

        // Verificar
        var asignacion = Assert.Single(asignaciones);
        Assert.Equal(1, asignacion.Id);
        Assert.Equal(1, asignacion.MateriaId);
        Assert.Equal("Matemática", asignacion.MateriaNombre);
        Assert.False(asignacion.EsTecnica);
        Assert.Null(asignacion.CantidadRa);
        Assert.Equal(1, asignacion.CursoId);
        Assert.Equal("1er Grado", asignacion.Grado);
        Assert.Equal("A", asignacion.Seccion);
        Assert.Equal(1, asignacion.AnioEscolarId);
        Assert.Equal("2026-2027", asignacion.AnioEscolarNombre);
        Assert.Equal(4, asignacion.HorasSemanales);
    }

    [Fact]
    public async Task MisAsignaciones_DocenteSinAsignaciones_DevuelveListaVacia()
    {
        // Preparar
        await using var contexto = CrearContexto();
        await SembrarEscenarioAsync(contexto);
        var servicio = new ConsultasDocenteServicio(contexto);

        // Actuar
        var asignaciones = await servicio.ListarMisAsignacionesAsync(999);

        // Verificar
        Assert.Empty(asignaciones);
    }

    [Fact]
    public async Task ListarEstudiantesDeCurso_CursoAsignado_DevuelveSoloLosDelCursoOrdenados()
    {
        // Preparar
        await using var contexto = CrearContexto();
        await SembrarEscenarioAsync(contexto);
        var servicio = new ConsultasDocenteServicio(contexto);

        // Actuar
        var estudiantes = await servicio.ListarEstudiantesDeCursoAsync(10, 1);

        // Verificar
        Assert.Equal(2, estudiantes.Count);
        Assert.Collection(estudiantes,
            e =>
            {
                Assert.Equal(2, e.Id);
                Assert.Equal("Luis", e.Nombre);
                Assert.Equal("Gómez", e.Apellido);
                Assert.Equal(1, e.CursoId);
            },
            e =>
            {
                Assert.Equal(1, e.Id);
                Assert.Equal("Ana", e.Nombre);
                Assert.Equal("Pérez", e.Apellido);
                Assert.Equal(1, e.CursoId);
            });
    }

    [Fact]
    public async Task ListarEstudiantesDeCurso_CursoExistenteNoAsignadoAlDocente_Lanza403()
    {
        // Preparar
        await using var contexto = CrearContexto();
        await SembrarEscenarioAsync(contexto);
        var servicio = new ConsultasDocenteServicio(contexto);

        // Actuar
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() =>
            servicio.ListarEstudiantesDeCursoAsync(10, 2));

        // Verificar
        Assert.Equal(403, excepcion.CodigoHttp);
        Assert.Equal("No tienes asignaciones en este curso.", excepcion.Message);
    }

    [Fact]
    public async Task ListarEstudiantesDeCurso_CursoInexistente_Lanza404()
    {
        // Preparar
        await using var contexto = CrearContexto();
        await SembrarEscenarioAsync(contexto);
        var servicio = new ConsultasDocenteServicio(contexto);

        // Actuar
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() =>
            servicio.ListarEstudiantesDeCursoAsync(10, 999));

        // Verificar
        Assert.Equal(404, excepcion.CodigoHttp);
        Assert.Equal("El curso no existe.", excepcion.Message);
    }

    [Fact]
    public async Task ObtenerAnioEscolarActivo_DevuelveElActivoAunqueExistaUnoInactivo()
    {
        // Preparar
        await using var contexto = CrearContexto();
        await SembrarEscenarioAsync(contexto);
        var servicio = new ConsultasDocenteServicio(contexto);

        // Actuar
        var anioActivo = await servicio.ObtenerAnioEscolarActivoAsync();

        // Verificar
        Assert.Equal(1, anioActivo.Id);
        Assert.Equal("2026-2027", anioActivo.Nombre);
        Assert.True(anioActivo.EsActivo);
        Assert.Equal(new DateOnly(2026, 8, 1), anioActivo.FechaInicio);
        Assert.Equal(new DateOnly(2027, 6, 30), anioActivo.FechaFin);
    }

    [Fact]
    public async Task ObtenerAnioEscolarActivo_SinAnioActivo_Lanza404()
    {
        // Preparar
        await using var contexto = CrearContexto();
        contexto.AniosEscolares.Add(new AnioEscolar
        {
            Id = 1,
            Nombre = "2025-2026",
            FechaInicio = new DateOnly(2025, 8, 1),
            FechaFin = new DateOnly(2026, 6, 30),
            EsActivo = false,
        });
        await contexto.SaveChangesAsync();
        var servicio = new ConsultasDocenteServicio(contexto);

        // Actuar
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() =>
            servicio.ObtenerAnioEscolarActivoAsync());

        // Verificar
        Assert.Equal(404, excepcion.CodigoHttp);
        Assert.Equal("No hay un año escolar activo.", excepcion.Message);
    }

    private static TamsDbContext CrearContexto()
    {
        var opciones = new DbContextOptionsBuilder<TamsDbContext>()
            .UseInMemoryDatabase($"ConsultasDocente-{Guid.NewGuid():N}")
            .Options;

        return new TamsDbContext(opciones);
    }

    private static async Task SembrarEscenarioAsync(TamsDbContext contexto)
    {
        var anioActivo = new AnioEscolar
        {
            Id = 1,
            Nombre = "2026-2027",
            FechaInicio = new DateOnly(2026, 8, 1),
            FechaFin = new DateOnly(2027, 6, 30),
            EsActivo = true,
        };

        var anioInactivo = new AnioEscolar
        {
            Id = 2,
            Nombre = "2025-2026",
            FechaInicio = new DateOnly(2025, 8, 1),
            FechaFin = new DateOnly(2026, 6, 30),
            EsActivo = false,
        };

        var curso1 = new Curso
        {
            Id = 1,
            Grado = "1er Grado",
            Seccion = "A",
            EsTecnico = false,
            AnioEscolarId = anioActivo.Id,
            AnioEscolar = anioActivo,
        };

        var curso2 = new Curso
        {
            Id = 2,
            Grado = "2do Grado",
            Seccion = "B",
            EsTecnico = false,
            AnioEscolarId = anioActivo.Id,
            AnioEscolar = anioActivo,
        };

        var materia1 = new Materia
        {
            Id = 1,
            Nombre = "Matemática",
            EsTecnica = false,
        };

        var materia2 = new Materia
        {
            Id = 2,
            Nombre = "Programación",
            EsTecnica = true,
            CantidadRA = 3,
        };

        var estudianteAna = new Estudiante
        {
            Id = 1,
            Nombre = "Ana",
            Apellido = "Pérez",
            CursoId = curso1.Id,
            Curso = curso1,
        };

        var estudianteLuis = new Estudiante
        {
            Id = 2,
            Nombre = "Luis",
            Apellido = "Gómez",
            CursoId = curso1.Id,
            Curso = curso1,
        };

        var estudianteMarta = new Estudiante
        {
            Id = 3,
            Nombre = "Marta",
            Apellido = "Díaz",
            CursoId = curso2.Id,
            Curso = curso2,
        };

        var asignacionDocente10 = new AsignacionDocente
        {
            Id = 1,
            UsuarioId = 10,
            MateriaId = materia1.Id,
            CursoId = curso1.Id,
            HorasSemanales = 4,
            Materia = materia1,
            Curso = curso1,
        };

        var asignacionDocente20 = new AsignacionDocente
        {
            Id = 2,
            UsuarioId = 20,
            MateriaId = materia2.Id,
            CursoId = curso2.Id,
            HorasSemanales = 3,
            Materia = materia2,
            Curso = curso2,
        };

        contexto.AniosEscolares.AddRange(anioActivo, anioInactivo);
        contexto.Cursos.AddRange(curso1, curso2);
        contexto.Materias.AddRange(materia1, materia2);
        contexto.Estudiantes.AddRange(estudianteAna, estudianteLuis, estudianteMarta);
        contexto.AsignacionesDocentes.AddRange(asignacionDocente10, asignacionDocente20);

        await contexto.SaveChangesAsync();
    }
}
