using Microsoft.EntityFrameworkCore;
using Tams.Negocio.Application.Servicios;
using Tams.Negocio.Domain.Entidades;
using Tams.Negocio.Domain.Enums;
using Tams.Negocio.Infrastructura.Persistence;

namespace Tests.Negocio.Dobles;

/// <summary>
/// RD-12: escenario en memoria con un nombre de base único por prueba.
/// </summary>
internal sealed class EscenarioReporteAsistencia : IAsyncDisposable
{
    public required TamsDbContext Contexto { get; init; }

    public required ReporteAsistenciaServicio Servicio { get; init; }

    public async ValueTask DisposeAsync()
    {
        await Contexto.DisposeAsync();
    }
}

internal static class ContextoReporteAsistencia
{
    public static EscenarioReporteAsistencia Crear()
    {
        var opciones = new DbContextOptionsBuilder<TamsDbContext>()
            .UseInMemoryDatabase($"ReporteAsistencia-{Guid.NewGuid():N}")
            .Options;
        var contexto = new TamsDbContext(opciones);

        return new EscenarioReporteAsistencia
        {
            Contexto = contexto,
            Servicio = new ReporteAsistenciaServicio(contexto)
        };
    }

    public static async Task SembrarEscenarioAsync(TamsDbContext contexto)
    {
        var anioActivo = new AnioEscolar
        {
            Id = 1,
            Nombre = "2026-2027",
            FechaInicio = new DateOnly(2026, 8, 1),
            FechaFin = new DateOnly(2027, 6, 30),
            EsActivo = true,
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

        var matematica = new Materia
        {
            Id = 1,
            Nombre = "Matemática",
            EsTecnica = false,
        };

        var historia = new Materia
        {
            Id = 2,
            Nombre = "Historia",
            EsTecnica = false,
        };

        var ana = new Estudiante
        {
            Id = 1,
            Nombre = "Ana",
            Apellido = "Pérez",
            CursoId = curso1.Id,
            Curso = curso1,
        };

        var luis = new Estudiante
        {
            Id = 2,
            Nombre = "Luis",
            Apellido = "Gómez",
            CursoId = curso1.Id,
            Curso = curso1,
        };

        var marta = new Estudiante
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
            MateriaId = matematica.Id,
            CursoId = curso1.Id,
            HorasSemanales = 4,
            Materia = matematica,
            Curso = curso1,
        };

        var asignacionDocente20 = new AsignacionDocente
        {
            Id = 2,
            UsuarioId = 20,
            MateriaId = historia.Id,
            CursoId = curso2.Id,
            HorasSemanales = 3,
            Materia = historia,
            Curso = curso2,
        };

        contexto.AniosEscolares.Add(anioActivo);
        contexto.Cursos.AddRange(curso1, curso2);
        contexto.Materias.AddRange(matematica, historia);
        contexto.Estudiantes.AddRange(ana, luis, marta);
        contexto.AsignacionesDocentes.AddRange(asignacionDocente10, asignacionDocente20);

        await contexto.SaveChangesAsync();
    }

    public static async Task SembrarAsistenciaAsync(
        TamsDbContext contexto,
        int asignacionDocenteId,
        int estudianteId,
        DateOnly fecha,
        EstadoAsistencia estado)
    {
        contexto.Asistencias.Add(new Asistencia
        {
            AsignacionDocenteId = asignacionDocenteId,
            EstudianteId = estudianteId,
            Fecha = fecha,
            Estado = estado,
        });

        await contexto.SaveChangesAsync();
    }
}
