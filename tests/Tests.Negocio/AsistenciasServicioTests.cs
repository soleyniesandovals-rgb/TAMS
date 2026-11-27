using Core.ControlAcceso.Domain.Excepciones;
using Microsoft.EntityFrameworkCore;
using Tams.Negocio.Application.Modelos;
using Tams.Negocio.Application.Servicios;
using Tams.Negocio.Domain.Entidades;
using Tams.Negocio.Domain.Enums;
using Tams.Negocio.Infrastructura.Persistence;
using Xunit;

namespace Tests.Negocio;

/// <summary>
/// Pruebas de la lista de asistencia del docente (RF-NEG-02, RD-02, RD-11, RD-12).
/// </summary>
public class AsistenciasServicioTests
{
    private static readonly DateOnly FechaLista = new(2026, 11, 2);
    private static readonly DateOnly Hoy = new(2026, 11, 14);

    [Fact]
    public async Task Obtener_SinGuardar_DevuelveTodosLosEstudiantesSinEstado()
    {
        // Preparar
        await using var contexto = CrearContexto();
        await SembrarEscenarioAsync(contexto);
        var servicio = CrearServicio(contexto);

        // Actuar
        var lista = await servicio.ObtenerListaAsync(10, 1, FechaLista);

        // Verificar
        Assert.False(lista.Guardada);
        Assert.Equal(4, lista.Registros.Count);
        Assert.All(lista.Registros, registro =>
        {
            Assert.Null(registro.Estado);
            Assert.Null(registro.Observacion);
        });
        Assert.Equal(
            ["Díaz", "Gómez", "Pérez", "Ruiz"],
            lista.Registros.Select(r => r.Apellido).ToList());
        Assert.Empty(await contexto.Asistencias.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task RF_NEG_AdministradorTitular_ObtenerLista_SoloVeLaDeSuAsignacion()
    {
        // Preparar: el usuario 10 es el Administrador titular de la asignación 1; la
        // asignación 3 pertenece a otro usuario (el rol vive en el token, no en la tabla).
        await using var contexto = CrearContexto();
        await SembrarEscenarioAsync(contexto);
        contexto.AsignacionesDocentes.Add(new AsignacionDocente
        {
            Id = 3,
            UsuarioId = 20,
            MateriaId = 1,
            CursoId = 1,
            HorasSemanales = 2,
        });
        await contexto.SaveChangesAsync();
        var servicio = CrearServicio(contexto);

        // Actuar: consulta su lista y la asignación que no le pertenece.
        var lista = await servicio.ObtenerListaAsync(10, 1, FechaLista);
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() =>
            servicio.ObtenerListaAsync(10, 3, FechaLista));

        // Verificar: ve su lista igual que un docente y la ajena le da 403.
        Assert.Equal(1, lista.AsignacionId);
        Assert.Equal(4, lista.Registros.Count);
        Assert.Equal(403, excepcion.CodigoHttp);
        Assert.Equal("No eres el docente de esta asignación.", excepcion.Message);
    }

    [Fact]
    public async Task Guardar_ListaValidaConLosCuatroEstados_GuardaYDevuelveGuardada()
    {
        // Preparar
        await using var contexto = CrearContexto();
        await SembrarEscenarioAsync(contexto);
        var servicio = CrearServicio(contexto);

        // Actuar
        var lista = await servicio.GuardarListaAsync(10, 1, FechaLista, RegistrosValidos());

        // Verificar
        Assert.True(lista.Guardada);
        Assert.Equal(4, lista.Registros.Count);
        Assert.Equal(4, await contexto.Asistencias.CountAsync());
        Assert.Equal(
            4,
            (await contexto.Asistencias.AsNoTracking().ToListAsync())
                .Select(a => a.Estado)
                .Distinct()
                .Count());
    }

    [Fact]
    public async Task Guardar_OtraVezConCambios_ActualizaSinDuplicarFilas()
    {
        // Preparar
        await using var contexto = CrearContexto();
        await SembrarEscenarioAsync(contexto);
        var servicio = CrearServicio(contexto);
        await servicio.GuardarListaAsync(10, 1, FechaLista, RegistrosValidos());

        // Actuar
        var lista = await servicio.GuardarListaAsync(
            10,
            1,
            FechaLista,
            [
                new(1, "Excusa", "Cita médica"),
                new(2, "Tardanza", "Llegó a las 8:20"),
                new(3, "Presente", null),
                new(4, "Ausente", "Sin aviso"),
            ]);

        // Verificar
        Assert.True(lista.Guardada);
        Assert.Equal(4, await contexto.Asistencias.CountAsync());
        var luis = await contexto.Asistencias
            .AsNoTracking()
            .SingleAsync(a => a.EstudianteId == 2);
        Assert.Equal(EstadoAsistencia.Tardanza, luis.Estado);
        Assert.Equal("Llegó a las 8:20", luis.Observacion);
    }

    [Fact]
    public async Task Obtener_DespuesDeGuardar_DevuelveEstadosYObservaciones()
    {
        // Preparar
        await using var contexto = CrearContexto();
        await SembrarEscenarioAsync(contexto);
        var servicio = CrearServicio(contexto);
        await servicio.GuardarListaAsync(10, 1, FechaLista, RegistrosValidos());

        // Actuar
        var lista = await servicio.ObtenerListaAsync(10, 1, FechaLista);

        // Verificar
        Assert.True(lista.Guardada);
        var porEstudiante = lista.Registros.ToDictionary(r => r.EstudianteId);
        Assert.Equal("Presente", porEstudiante[1].Estado);
        Assert.Null(porEstudiante[1].Observacion);
        Assert.Equal("Ausente", porEstudiante[2].Estado);
        Assert.Equal("Sin aviso", porEstudiante[2].Observacion);
        Assert.Equal("Tardanza", porEstudiante[3].Estado);
        Assert.Equal("Llegó tarde", porEstudiante[3].Observacion);
        Assert.Equal("Excusa", porEstudiante[4].Estado);
        Assert.Equal("Cita médica", porEstudiante[4].Observacion);
    }

    [Fact]
    public async Task Guardar_AsignacionInexistente_Lanza404YNoGuarda()
    {
        // Preparar
        await using var contexto = CrearContexto();
        await SembrarEscenarioAsync(contexto);
        var servicio = CrearServicio(contexto);

        // Actuar
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() =>
            servicio.GuardarListaAsync(10, 999, FechaLista, RegistrosValidos()));

        // Verificar
        Assert.Equal(404, excepcion.CodigoHttp);
        Assert.Equal("La asignación no existe.", excepcion.Message);
        Assert.Empty(await contexto.Asistencias.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task Guardar_AsignacionDeOtroDocente_Lanza403YNoGuarda()
    {
        // Preparar
        await using var contexto = CrearContexto();
        await SembrarEscenarioAsync(contexto);
        var servicio = CrearServicio(contexto);

        // Actuar
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() =>
            servicio.GuardarListaAsync(20, 1, FechaLista, RegistrosValidos()));

        // Verificar
        Assert.Equal(403, excepcion.CodigoHttp);
        Assert.Equal("No eres el docente de esta asignación.", excepcion.Message);
        Assert.Empty(await contexto.Asistencias.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task Guardar_CursoDeAnioNoActivo_Lanza400YNoGuarda()
    {
        // Preparar
        await using var contexto = CrearContexto();
        await SembrarEscenarioAsync(contexto);
        var servicio = CrearServicio(contexto);

        // Actuar
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() =>
            servicio.GuardarListaAsync(
                10,
                2,
                FechaLista,
                [new(99, "Presente", null)]));

        // Verificar
        Assert.Equal(400, excepcion.CodigoHttp);
        Assert.Equal("Solo se pasa lista en cursos del año escolar activo.", excepcion.Message);
        Assert.Empty(await contexto.Asistencias.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task Guardar_FechaFutura_Lanza400YNoGuarda()
    {
        // Preparar
        await using var contexto = CrearContexto();
        await SembrarEscenarioAsync(contexto);
        var servicio = CrearServicio(contexto);

        // Actuar
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() =>
            servicio.GuardarListaAsync(10, 1, Hoy.AddDays(1), RegistrosValidos()));

        // Verificar
        Assert.Equal(400, excepcion.CodigoHttp);
        Assert.Equal("No se puede pasar lista de una fecha futura.", excepcion.Message);
        Assert.Empty(await contexto.Asistencias.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task Guardar_FechaFueraDelAnioActivo_Lanza400YNoGuarda()
    {
        // Preparar
        await using var contexto = CrearContexto();
        await SembrarEscenarioAsync(contexto);
        var servicio = CrearServicio(contexto);

        // Actuar
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() =>
            servicio.GuardarListaAsync(
                10,
                1,
                new DateOnly(2026, 7, 31),
                RegistrosValidos()));

        // Verificar
        Assert.Equal(400, excepcion.CodigoHttp);
        Assert.Equal("La fecha debe estar dentro del año escolar activo.", excepcion.Message);
        Assert.Empty(await contexto.Asistencias.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task Guardar_FaltaUnEstudiante_Lanza400ConCantidadYNoGuarda()
    {
        // Preparar
        await using var contexto = CrearContexto();
        await SembrarEscenarioAsync(contexto);
        var servicio = CrearServicio(contexto);

        // Actuar
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() =>
            servicio.GuardarListaAsync(
                10,
                1,
                FechaLista,
                [
                    new(1, "Presente", null),
                    new(2, "Ausente", null),
                    new(3, "Tardanza", null),
                ]));

        // Verificar
        Assert.Equal(400, excepcion.CodigoHttp);
        Assert.Equal(
            "La lista debe incluir a todos los estudiantes del curso (faltan 1).",
            excepcion.Message);
        Assert.Empty(await contexto.Asistencias.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task Guardar_EstudianteRepetido_Lanza400YNoGuarda()
    {
        // Preparar
        await using var contexto = CrearContexto();
        await SembrarEscenarioAsync(contexto);
        var servicio = CrearServicio(contexto);

        // Actuar
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() =>
            servicio.GuardarListaAsync(
                10,
                1,
                FechaLista,
                [
                    new(1, "Presente", null),
                    new(1, "Ausente", null),
                    new(2, "Tardanza", null),
                    new(3, "Excusa", null),
                ]));

        // Verificar
        Assert.Equal(400, excepcion.CodigoHttp);
        Assert.Equal("Cada estudiante debe aparecer una sola vez.", excepcion.Message);
        Assert.Empty(await contexto.Asistencias.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task Guardar_EstudianteDeOtroCurso_Lanza400YNoGuarda()
    {
        // Preparar
        await using var contexto = CrearContexto();
        await SembrarEscenarioAsync(contexto);
        var servicio = CrearServicio(contexto);

        // Actuar
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() =>
            servicio.GuardarListaAsync(
                10,
                1,
                FechaLista,
                [
                    new(1, "Presente", null),
                    new(2, "Ausente", null),
                    new(3, "Tardanza", null),
                    new(4, "Excusa", null),
                    new(99, "Presente", null),
                ]));

        // Verificar
        Assert.Equal(400, excepcion.CodigoHttp);
        Assert.Equal(
            "La lista solo puede incluir estudiantes del curso.",
            excepcion.Message);
        Assert.Empty(await contexto.Asistencias.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task Guardar_EstadoNoDefinido_Lanza400YNoGuarda()
    {
        // Preparar
        await using var contexto = CrearContexto();
        await SembrarEscenarioAsync(contexto);
        var servicio = CrearServicio(contexto);

        // Actuar
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() =>
            servicio.GuardarListaAsync(
                10,
                1,
                FechaLista,
                [
                    new(1, "Presente", null),
                    new(2, "Ausente", null),
                    new(3, "Tardanza", null),
                    new(4, "Enfermo", null),
                ]));

        // Verificar
        Assert.Equal(400, excepcion.CodigoHttp);
        Assert.Equal(
            "El estado debe ser Presente, Ausente, Tardanza o Excusa.",
            excepcion.Message);
        Assert.Empty(await contexto.Asistencias.AsNoTracking().ToListAsync());
    }

    [Fact]
    public void RegistroAsistenciaResumen_NoExponeCedula()
    {
        // Actuar
        var propiedad = typeof(RegistroAsistenciaResumen).GetProperty("Cedula");

        // Verificar
        Assert.Null(propiedad);
    }

    private static AsistenciasServicio CrearServicio(TamsDbContext contexto)
        => new(contexto, new TimeProviderFijo(
            new DateTimeOffset(2026, 11, 14, 12, 0, 0, TimeSpan.Zero)));

    private static RegistroAsistenciaEntrada[] RegistrosValidos()
        =>
        [
            new(1, "Presente", null),
            new(2, "Ausente", "Sin aviso"),
            new(3, "Tardanza", "Llegó tarde"),
            new(4, "Excusa", "Cita médica"),
        ];

    private static TamsDbContext CrearContexto()
    {
        var opciones = new DbContextOptionsBuilder<TamsDbContext>()
            .UseInMemoryDatabase($"Asistencias-{Guid.NewGuid():N}")
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

        var cursoActivo = new Curso
        {
            Id = 1,
            Grado = "1er Grado",
            Seccion = "A",
            AnioEscolarId = anioActivo.Id,
            AnioEscolar = anioActivo,
        };

        var cursoInactivo = new Curso
        {
            Id = 2,
            Grado = "2do Grado",
            Seccion = "B",
            AnioEscolarId = anioInactivo.Id,
            AnioEscolar = anioInactivo,
        };

        var matematica = new Materia
        {
            Id = 1,
            Nombre = "Matemática",
        };

        var estudiantes = new[]
        {
            new Estudiante
            {
                Id = 1,
                Nombre = "Ana",
                Apellido = "Pérez",
                CursoId = cursoActivo.Id,
                Curso = cursoActivo,
            },
            new Estudiante
            {
                Id = 2,
                Nombre = "Luis",
                Apellido = "Gómez",
                CursoId = cursoActivo.Id,
                Curso = cursoActivo,
            },
            new Estudiante
            {
                Id = 3,
                Nombre = "Marta",
                Apellido = "Díaz",
                CursoId = cursoActivo.Id,
                Curso = cursoActivo,
            },
            new Estudiante
            {
                Id = 4,
                Nombre = "Pedro",
                Apellido = "Ruiz",
                CursoId = cursoActivo.Id,
                Curso = cursoActivo,
            },
            new Estudiante
            {
                Id = 99,
                Nombre = "Sofía",
                Apellido = "Otro",
                CursoId = cursoInactivo.Id,
                Curso = cursoInactivo,
            },
        };

        var asignacionActiva = new AsignacionDocente
        {
            Id = 1,
            UsuarioId = 10,
            MateriaId = matematica.Id,
            CursoId = cursoActivo.Id,
            HorasSemanales = 4,
            Materia = matematica,
            Curso = cursoActivo,
        };

        var asignacionInactiva = new AsignacionDocente
        {
            Id = 2,
            UsuarioId = 10,
            MateriaId = matematica.Id,
            CursoId = cursoInactivo.Id,
            HorasSemanales = 4,
            Materia = matematica,
            Curso = cursoInactivo,
        };

        contexto.AniosEscolares.AddRange(anioActivo, anioInactivo);
        contexto.Cursos.AddRange(cursoActivo, cursoInactivo);
        contexto.Materias.Add(matematica);
        contexto.Estudiantes.AddRange(estudiantes);
        contexto.AsignacionesDocentes.AddRange(asignacionActiva, asignacionInactiva);

        await contexto.SaveChangesAsync();
    }

    private sealed class TimeProviderFijo(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
