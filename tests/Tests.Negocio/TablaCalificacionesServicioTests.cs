using Core.ControlAcceso.Domain.Excepciones;
using Microsoft.EntityFrameworkCore;
using Tams.Negocio.Application.Servicios;
using Tams.Negocio.Domain.Entidades;
using Tams.Negocio.Domain.Enums;
using Tams.Negocio.Infrastructura.Persistence;
using Xunit;

namespace Tests.Negocio;

/// <summary>
/// Pruebas de la tabla de calificaciones con acumulado por estudiante (RF-NEG, RD-02, RD-06).
/// </summary>
public class TablaCalificacionesServicioTests
{
    [Fact]
    public async Task RF_NEG_TablaCalificaciones_Basica_DosEstudiantesUnoSinNotas()
    {
        // Preparar
        await using var contexto = CrearContexto();
        await SembrarEscenarioAsync(contexto);
        contexto.Calificaciones.AddRange(
            NuevaNota(1, 1, 1, 1, TipoEvaluacion.Periodo, 1, 80m, EstadoCalificacion.Borrador),
            NuevaNota(2, 1, 1, 1, TipoEvaluacion.Periodo, 2, 90m, EstadoCalificacion.Publicada));
        await contexto.SaveChangesAsync();
        var servicio = new ConsultasDocenteServicio(contexto);

        // Actuar
        var tabla = await servicio.ObtenerTablaCalificacionesAsync(10, 1);

        // Verificar
        Assert.Equal(1, tabla.AsignacionId);
        Assert.Equal(1, tabla.MateriaId);
        Assert.Equal("Matemática", tabla.MateriaNombre);
        Assert.False(tabla.EsTecnica);
        Assert.Equal("1er Grado", tabla.Grado);
        Assert.Equal("A", tabla.Seccion);
        Assert.Equal(1, tabla.AnioEscolarId);
        Assert.Equal("2026-2027", tabla.AnioEscolarNombre);
        Assert.Equal(4, tabla.Columnas.Count);
        Assert.All(tabla.Columnas, columna =>
        {
            Assert.Equal(TipoEvaluacion.Periodo, columna.TipoEvaluacion);
            Assert.Null(columna.Puntos);
        });
        Assert.Equal(
            ["P1", "P2", "P3", "P4"],
            tabla.Columnas.Select(c => c.Etiqueta).ToArray());
        Assert.Equal(2, tabla.Estudiantes.Count);
        var luis = Assert.Single(tabla.Estudiantes, e => e.EstudianteId == 2);
        Assert.Equal("Luis", luis.Nombre);
        Assert.Equal("Gómez", luis.Apellido);
        Assert.Empty(luis.Notas);
        Assert.Null(luis.Acumulado);
        var ana = Assert.Single(tabla.Estudiantes, e => e.EstudianteId == 1);
        Assert.Equal(2, ana.Notas.Count);
        Assert.Equal([1, 2], ana.Notas.Select(n => n.Numero).ToArray());
        Assert.Equal(80m, ana.Notas[0].PuntajeEfectivo);
        Assert.Equal("Borrador", ana.Notas[0].Estado);
        Assert.Equal(85m, ana.Acumulado);
    }

    [Fact]
    public async Task RF_NEG_TablaCalificaciones_Tecnica_ColumnasPorRaYAcumuladoPonderado()
    {
        // Preparar
        await using var contexto = CrearContexto();
        await SembrarEscenarioAsync(contexto);
        contexto.Calificaciones.AddRange(
            NuevaNota(1, 3, 2, 1, TipoEvaluacion.RA, 1, 80m, EstadoCalificacion.Borrador),
            NuevaNota(2, 3, 2, 1, TipoEvaluacion.RA, 2, 70m, EstadoCalificacion.Enviada));
        await contexto.SaveChangesAsync();
        var servicio = new ConsultasDocenteServicio(contexto);

        // Actuar
        var tabla = await servicio.ObtenerTablaCalificacionesAsync(20, 2);

        // Verificar
        Assert.True(tabla.EsTecnica);
        Assert.Equal(
            ["RA1", "RA2", "RA3"],
            tabla.Columnas.Select(c => c.Etiqueta).ToArray());
        Assert.Equal([34, 33, 33], tabla.Columnas.Select(c => c.Puntos).ToArray());
        Assert.All(tabla.Columnas, c => Assert.Equal(TipoEvaluacion.RA, c.TipoEvaluacion));
        var marta = Assert.Single(tabla.Estudiantes);
        Assert.Equal(3, marta.EstudianteId);
        Assert.Equal(2, marta.Notas.Count);
        Assert.Equal(80m, marta.Notas[0].PuntajeEfectivo);
        Assert.Equal("Borrador", marta.Notas[0].Estado);
        Assert.Equal(50.30m, marta.Acumulado);
    }

    [Fact]
    public async Task RF_NEG_TablaCalificaciones_SoloIncluyeLasNotasDeMateriaAnioYCurso()
    {
        // Preparar: una nota válida y tres que deben quedar fuera del filtro.
        await using var contexto = CrearContexto();
        await SembrarEscenarioAsync(contexto);
        contexto.Calificaciones.AddRange(
            NuevaNota(1, 1, 1, 1, TipoEvaluacion.Periodo, 1, 80m, EstadoCalificacion.Borrador),
            NuevaNota(2, 1, 3, 1, TipoEvaluacion.Periodo, 2, 100m, EstadoCalificacion.Borrador),
            NuevaNota(3, 1, 1, 2, TipoEvaluacion.Periodo, 3, 100m, EstadoCalificacion.Borrador),
            NuevaNota(4, 4, 1, 1, TipoEvaluacion.Periodo, 4, 100m, EstadoCalificacion.Borrador));
        await contexto.SaveChangesAsync();
        var servicio = new ConsultasDocenteServicio(contexto);

        // Actuar
        var tabla = await servicio.ObtenerTablaCalificacionesAsync(10, 1);

        // Verificar
        Assert.Equal(2, tabla.Estudiantes.Count);
        Assert.DoesNotContain(tabla.Estudiantes, e => e.EstudianteId == 4);
        var ana = Assert.Single(tabla.Estudiantes, e => e.EstudianteId == 1);
        var nota = Assert.Single(ana.Notas);
        Assert.Equal(1, nota.Numero);
        Assert.Equal(1, nota.CalificacionId);
        Assert.Equal(80m, nota.PuntajeEfectivo);
        Assert.Equal(80m, ana.Acumulado);
        Assert.All(tabla.Estudiantes, e => Assert.DoesNotContain(e.Notas, n => n.Numero > 1));
    }

    [Fact]
    public async Task RF_NEG_TablaCalificaciones_TitularAdministrador_VeLaSuya()
    {
        // Preparar: el rol vive en el token; el servicio solo comprueba titularidad por usuarioId.
        await using var contexto = CrearContexto();
        await SembrarEscenarioAsync(contexto);
        var servicio = new ConsultasDocenteServicio(contexto);

        // Actuar
        var tabla = await servicio.ObtenerTablaCalificacionesAsync(20, 2);

        // Verificar
        Assert.Equal(2, tabla.AsignacionId);
        Assert.Equal("Programación", tabla.MateriaNombre);
        Assert.Equal("2do Grado", tabla.Grado);
        Assert.Equal("B", tabla.Seccion);
        Assert.Equal(3, tabla.Columnas.Count);
        var marta = Assert.Single(tabla.Estudiantes);
        Assert.Equal(3, marta.EstudianteId);
        Assert.Empty(marta.Notas);
        Assert.Null(marta.Acumulado);
    }

    [Fact]
    public async Task RF_NEG_TablaCalificaciones_UsuarioNoTitular_Lanza403()
    {
        // Preparar
        await using var contexto = CrearContexto();
        await SembrarEscenarioAsync(contexto);
        var servicio = new ConsultasDocenteServicio(contexto);

        // Actuar
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() =>
            servicio.ObtenerTablaCalificacionesAsync(10, 2));

        // Verificar
        Assert.Equal(403, excepcion.CodigoHttp);
        Assert.Equal("No eres el docente de esta asignación.", excepcion.Message);
    }

    [Fact]
    public async Task RF_NEG_TablaCalificaciones_AsignacionInexistente_Lanza404()
    {
        // Preparar
        await using var contexto = CrearContexto();
        await SembrarEscenarioAsync(contexto);
        var servicio = new ConsultasDocenteServicio(contexto);

        // Actuar
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() =>
            servicio.ObtenerTablaCalificacionesAsync(10, 999));

        // Verificar
        Assert.Equal(404, excepcion.CodigoHttp);
        Assert.Equal("La asignación no existe.", excepcion.Message);
    }

    private static TamsDbContext CrearContexto()
    {
        var opciones = new DbContextOptionsBuilder<TamsDbContext>()
            .UseInMemoryDatabase($"TablaCalificaciones-{Guid.NewGuid():N}")
            .Options;

        return new TamsDbContext(opciones);
    }

    private static Calificacion NuevaNota(
        int id,
        int estudianteId,
        int materiaId,
        int anioEscolarId,
        TipoEvaluacion tipo,
        int numero,
        decimal obtenido,
        EstadoCalificacion estado,
        decimal? recuperacion = null)
        => new()
        {
            Id = id,
            EstudianteId = estudianteId,
            MateriaId = materiaId,
            AnioEscolarId = anioEscolarId,
            TipoEvaluacion = tipo,
            Periodo = tipo == TipoEvaluacion.Periodo ? numero : null,
            NumeroRA = tipo == TipoEvaluacion.RA ? numero : null,
            PuntajeObtenido = obtenido,
            PuntajeRecuperacion = recuperacion,
            Estado = estado,
        };

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

        var anioAnterior = new AnioEscolar
        {
            Id = 2,
            Nombre = "2025-2026",
            FechaInicio = new DateOnly(2025, 8, 1),
            FechaFin = new DateOnly(2026, 6, 30),
            EsActivo = false,
        };

        var cursoBasico = new Curso
        {
            Id = 1,
            Grado = "1er Grado",
            Seccion = "A",
            AnioEscolarId = anioActivo.Id,
            AnioEscolar = anioActivo,
        };

        var cursoTecnico = new Curso
        {
            Id = 2,
            Grado = "2do Grado",
            Seccion = "B",
            EsTecnico = true,
            AnioEscolarId = anioActivo.Id,
            AnioEscolar = anioActivo,
        };

        var cursoAjeno = new Curso
        {
            Id = 3,
            Grado = "3er Grado",
            Seccion = "C",
            AnioEscolarId = anioActivo.Id,
            AnioEscolar = anioActivo,
        };

        var matematica = new Materia { Id = 1, Nombre = "Matemática", EsTecnica = false };
        var programacion = new Materia
        {
            Id = 2,
            Nombre = "Programación",
            EsTecnica = true,
            CantidadRA = 3,
        };
        var otraMateria = new Materia { Id = 3, Nombre = "Historia", EsTecnica = false };

        var ana = new Estudiante
        {
            Id = 1,
            Nombre = "Ana",
            Apellido = "Pérez",
            CursoId = cursoBasico.Id,
            Curso = cursoBasico,
        };
        var luis = new Estudiante
        {
            Id = 2,
            Nombre = "Luis",
            Apellido = "Gómez",
            CursoId = cursoBasico.Id,
            Curso = cursoBasico,
        };
        var marta = new Estudiante
        {
            Id = 3,
            Nombre = "Marta",
            Apellido = "Díaz",
            CursoId = cursoTecnico.Id,
            Curso = cursoTecnico,
        };
        var elena = new Estudiante
        {
            Id = 4,
            Nombre = "Elena",
            Apellido = "Ruiz",
            CursoId = cursoAjeno.Id,
            Curso = cursoAjeno,
        };

        var asignacionBasica = new AsignacionDocente
        {
            Id = 1,
            UsuarioId = 10,
            MateriaId = matematica.Id,
            CursoId = cursoBasico.Id,
            HorasSemanales = 4,
            Materia = matematica,
            Curso = cursoBasico,
        };
        var asignacionTecnica = new AsignacionDocente
        {
            Id = 2,
            UsuarioId = 20,
            MateriaId = programacion.Id,
            CursoId = cursoTecnico.Id,
            HorasSemanales = 3,
            Materia = programacion,
            Curso = cursoTecnico,
        };

        var ra1 = new ResultadoAprendizaje { Id = 1, MateriaId = programacion.Id, Numero = 1, Puntos = 34 };
        var ra2 = new ResultadoAprendizaje { Id = 2, MateriaId = programacion.Id, Numero = 2, Puntos = 33 };
        var ra3 = new ResultadoAprendizaje { Id = 3, MateriaId = programacion.Id, Numero = 3, Puntos = 33 };

        contexto.AniosEscolares.AddRange(anioActivo, anioAnterior);
        contexto.Cursos.AddRange(cursoBasico, cursoTecnico, cursoAjeno);
        contexto.Materias.AddRange(matematica, programacion, otraMateria);
        contexto.Estudiantes.AddRange(ana, luis, marta, elena);
        contexto.AsignacionesDocentes.AddRange(asignacionBasica, asignacionTecnica);
        contexto.ResultadosAprendizaje.AddRange(ra1, ra2, ra3);

        await contexto.SaveChangesAsync();
    }
}
