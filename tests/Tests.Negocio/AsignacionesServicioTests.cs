using Core.ControlAcceso.Application.Interfaces;
using Core.ControlAcceso.Application.Modelos;
using Core.ControlAcceso.Domain.Excepciones;
using Microsoft.EntityFrameworkCore;
using Tams.Negocio.Application.Servicios;
using Tams.Negocio.Domain.Entidades;
using Tams.Negocio.Domain.Enums;
using Tams.Negocio.Infrastructura.Persistence;
using Xunit;

namespace Tests.Negocio;

/// <summary>
/// Pruebas del alta y listado de asignaciones docentes
/// (RF-NEG-02, RD-02, RD-03, RD-12).
/// </summary>
public class AsignacionesServicioTests
{
    [Fact]
    public async Task Crear_AsignacionValida_GuardaYDevuelveLosDatos()
    {
        // Preparar
        await using var contexto = CrearContexto();
        var anio = await SembrarAnioAsync(contexto);
        var materia = await SembrarMateriaAsync(contexto, "Matemática");
        var curso = await SembrarCursoAsync(contexto, anio.Id, "1er Grado", "A");
        var consulta = new ConsultaUsuariosFalsa(Docente(2, "Ana Docente"));
        var servicio = new AsignacionesServicio(contexto, consulta);

        // Actuar
        var resumen = await servicio.CrearAsync(2, materia.Id, curso.Id, 5);

        // Verificar
        Assert.True(resumen.Id > 0);
        Assert.Equal(2, resumen.UsuarioId);
        Assert.Equal("Ana Docente", resumen.DocenteNombre);
        Assert.Equal("docente2@ejemplo.com", resumen.DocenteCorreo);
        Assert.Equal(materia.Id, resumen.MateriaId);
        Assert.Equal("Matemática", resumen.MateriaNombre);
        Assert.Equal(curso.Id, resumen.CursoId);
        Assert.Equal("1er Grado", resumen.Grado);
        Assert.Equal("A", resumen.Seccion);
        Assert.Equal(anio.Id, resumen.AnioEscolarId);
        Assert.Equal("2026-2027", resumen.AnioEscolarNombre);
        Assert.Equal(5, resumen.HorasSemanales);

        var guardada = await contexto.AsignacionesDocentes.AsNoTracking().SingleAsync();
        Assert.Equal(2, guardada.UsuarioId);
        Assert.Equal(materia.Id, guardada.MateriaId);
        Assert.Equal(curso.Id, guardada.CursoId);
        Assert.Equal(5, guardada.HorasSemanales);
    }

    [Fact]
    public async Task Crear_ConCeroHoras_Lanza400YNoGuarda()
    {
        // Preparar
        await using var contexto = CrearContexto();
        var servicio = new AsignacionesServicio(contexto, new ConsultaUsuariosFalsa());

        // Actuar
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() =>
            servicio.CrearAsync(2, 1, 1, 0));

        // Verificar
        Assert.Equal(400, excepcion.CodigoHttp);
        Assert.Equal("Las horas semanales deben estar entre 1 y 40.", excepcion.Message);
        Assert.Empty(await contexto.AsignacionesDocentes.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task Crear_Con41Horas_Lanza400YNoGuarda()
    {
        // Preparar
        await using var contexto = CrearContexto();
        var servicio = new AsignacionesServicio(contexto, new ConsultaUsuariosFalsa());

        // Actuar
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() =>
            servicio.CrearAsync(2, 1, 1, 41));

        // Verificar
        Assert.Equal(400, excepcion.CodigoHttp);
        Assert.Equal("Las horas semanales deben estar entre 1 y 40.", excepcion.Message);
        Assert.Empty(await contexto.AsignacionesDocentes.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task Crear_ConMateriaInexistente_Lanza404YNoGuarda()
    {
        // Preparar
        await using var contexto = CrearContexto();
        var anio = await SembrarAnioAsync(contexto);
        var curso = await SembrarCursoAsync(contexto, anio.Id, "1er Grado", "A");
        var consulta = new ConsultaUsuariosFalsa(Docente(2, "Ana Docente"));
        var servicio = new AsignacionesServicio(contexto, consulta);

        // Actuar
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() =>
            servicio.CrearAsync(2, 9999, curso.Id, 4));

        // Verificar
        Assert.Equal(404, excepcion.CodigoHttp);
        Assert.Equal("La materia no existe.", excepcion.Message);
        Assert.Empty(await contexto.AsignacionesDocentes.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task Crear_ConCursoInexistente_Lanza404YNoGuarda()
    {
        // Preparar
        await using var contexto = CrearContexto();
        await SembrarAnioAsync(contexto);
        var materia = await SembrarMateriaAsync(contexto, "Matemática");
        var consulta = new ConsultaUsuariosFalsa(Docente(2, "Ana Docente"));
        var servicio = new AsignacionesServicio(contexto, consulta);

        // Actuar
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() =>
            servicio.CrearAsync(2, materia.Id, 9999, 4));

        // Verificar
        Assert.Equal(404, excepcion.CodigoHttp);
        Assert.Equal("El curso no existe.", excepcion.Message);
        Assert.Empty(await contexto.AsignacionesDocentes.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task Crear_ConDocenteInexistente_Lanza404YNoGuarda()
    {
        // Preparar
        await using var contexto = CrearContexto();
        var anio = await SembrarAnioAsync(contexto);
        var materia = await SembrarMateriaAsync(contexto, "Matemática");
        var curso = await SembrarCursoAsync(contexto, anio.Id, "1er Grado", "A");
        var servicio = new AsignacionesServicio(contexto, new ConsultaUsuariosFalsa());

        // Actuar
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() =>
            servicio.CrearAsync(999, materia.Id, curso.Id, 4));

        // Verificar
        Assert.Equal(404, excepcion.CodigoHttp);
        Assert.Equal("El docente no existe.", excepcion.Message);
        Assert.Empty(await contexto.AsignacionesDocentes.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task Crear_ConUsuarioAdministrador_Lanza400YNoGuarda()
    {
        // Preparar
        await using var contexto = CrearContexto();
        var anio = await SembrarAnioAsync(contexto);
        var materia = await SembrarMateriaAsync(contexto, "Matemática");
        var curso = await SembrarCursoAsync(contexto, anio.Id, "1er Grado", "A");
        var consulta = new ConsultaUsuariosFalsa(Usuario(1, "Administrador", "Administrador"));
        var servicio = new AsignacionesServicio(contexto, consulta);

        // Actuar
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() =>
            servicio.CrearAsync(1, materia.Id, curso.Id, 4));

        // Verificar
        Assert.Equal(400, excepcion.CodigoHttp);
        Assert.Equal(
            "El usuario debe ser un docente (rol Estándar) con la cuenta activa.",
            excepcion.Message);
        Assert.Empty(await contexto.AsignacionesDocentes.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task Crear_ConDocenteInactivo_Lanza400YNoGuarda()
    {
        // Preparar
        await using var contexto = CrearContexto();
        var anio = await SembrarAnioAsync(contexto);
        var materia = await SembrarMateriaAsync(contexto, "Matemática");
        var curso = await SembrarCursoAsync(contexto, anio.Id, "1er Grado", "A");
        var consulta = new ConsultaUsuariosFalsa(Docente(2, "Ana Docente", activo: false));
        var servicio = new AsignacionesServicio(contexto, consulta);

        // Actuar
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() =>
            servicio.CrearAsync(2, materia.Id, curso.Id, 4));

        // Verificar
        Assert.Equal(400, excepcion.CodigoHttp);
        Assert.Equal(
            "El usuario debe ser un docente (rol Estándar) con la cuenta activa.",
            excepcion.Message);
        Assert.Empty(await contexto.AsignacionesDocentes.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task Crear_ConMateriaTecnicaEnCursoNoTecnico_Lanza400YNoGuarda()
    {
        // Preparar
        await using var contexto = CrearContexto();
        var anio = await SembrarAnioAsync(contexto);
        var materia = await SembrarMateriaAsync(
            contexto,
            "Programación",
            esTecnica: true,
            TipoTecnico.DesarrolloAdministracionAplicacionesInformaticas);
        var curso = await SembrarCursoAsync(contexto, anio.Id, "1er Grado", "A");
        var consulta = new ConsultaUsuariosFalsa(Docente(2, "Ana Docente"));
        var servicio = new AsignacionesServicio(contexto, consulta);

        // Actuar
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() =>
            servicio.CrearAsync(2, materia.Id, curso.Id, 4));

        // Verificar
        Assert.Equal(400, excepcion.CodigoHttp);
        Assert.Equal(
            "Una materia técnica solo puede asignarse a un curso técnico.",
            excepcion.Message);
        Assert.Empty(await contexto.AsignacionesDocentes.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task Crear_ConTiposTecnicosDistintos_Lanza400YNoGuarda()
    {
        // Preparar
        await using var contexto = CrearContexto();
        var anio = await SembrarAnioAsync(contexto);
        var materia = await SembrarMateriaAsync(
            contexto,
            "Comercio",
            esTecnica: true,
            TipoTecnico.ComercioYMercadeo);
        var curso = await SembrarCursoAsync(
            contexto,
            anio.Id,
            "1er Grado",
            "A",
            esTecnico: true,
            TipoTecnico.DesarrolloAdministracionAplicacionesInformaticas);
        var consulta = new ConsultaUsuariosFalsa(Docente(2, "Ana Docente"));
        var servicio = new AsignacionesServicio(contexto, consulta);

        // Actuar
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() =>
            servicio.CrearAsync(2, materia.Id, curso.Id, 4));

        // Verificar
        Assert.Equal(400, excepcion.CodigoHttp);
        Assert.Equal(
            "El tipo técnico de la materia y del curso deben coincidir.",
            excepcion.Message);
        Assert.Empty(await contexto.AsignacionesDocentes.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task Crear_MateriaYaAsignadaEnElCurso_Lanza409YNoDuplica()
    {
        // Preparar
        await using var contexto = CrearContexto();
        var anio = await SembrarAnioAsync(contexto);
        var materia = await SembrarMateriaAsync(contexto, "Matemática");
        var curso = await SembrarCursoAsync(contexto, anio.Id, "1er Grado", "A");
        var consulta = new ConsultaUsuariosFalsa(
            Docente(2, "Ana Docente"),
            Docente(3, "Luis Docente"));
        var servicio = new AsignacionesServicio(contexto, consulta);
        await servicio.CrearAsync(2, materia.Id, curso.Id, 4);

        // Actuar
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() =>
            servicio.CrearAsync(3, materia.Id, curso.Id, 5));

        // Verificar
        Assert.Equal(409, excepcion.CodigoHttp);
        Assert.Equal(
            "Esa materia ya tiene un docente asignado en este curso.",
            excepcion.Message);
        Assert.Equal(1, await contexto.AsignacionesDocentes.CountAsync());
    }

    [Fact]
    public async Task Crear_MismaMateriaEnOtraSeccion_SeCrea()
    {
        // Preparar
        await using var contexto = CrearContexto();
        var anio = await SembrarAnioAsync(contexto);
        var materia = await SembrarMateriaAsync(
            contexto,
            "Programación",
            esTecnica: true,
            TipoTecnico.DesarrolloAdministracionAplicacionesInformaticas);
        var cursoA = await SembrarCursoAsync(
            contexto,
            anio.Id,
            "1er Grado",
            "A",
            esTecnico: true,
            TipoTecnico.DesarrolloAdministracionAplicacionesInformaticas);
        var cursoB = await SembrarCursoAsync(
            contexto,
            anio.Id,
            "1er Grado",
            "B",
            esTecnico: true,
            TipoTecnico.DesarrolloAdministracionAplicacionesInformaticas);
        var consulta = new ConsultaUsuariosFalsa(
            Docente(2, "Ana Docente"),
            Docente(3, "Luis Docente"));
        var servicio = new AsignacionesServicio(contexto, consulta);

        // Actuar
        var primera = await servicio.CrearAsync(2, materia.Id, cursoA.Id, 4);
        var segunda = await servicio.CrearAsync(3, materia.Id, cursoB.Id, 6);

        // Verificar
        Assert.Equal("A", primera.Seccion);
        Assert.Equal("B", segunda.Seccion);
        Assert.Equal(2, await contexto.AsignacionesDocentes.CountAsync());
    }

    [Fact]
    public async Task Listar_ConFiltroDeCurso_DevuelveSoloEseCursoConNombres()
    {
        // Preparar
        await using var contexto = CrearContexto();
        var anio = await SembrarAnioAsync(contexto);
        var matematica = await SembrarMateriaAsync(contexto, "Matemática");
        var programacion = await SembrarMateriaAsync(contexto, "Programación");
        var cursoA = await SembrarCursoAsync(contexto, anio.Id, "1er Grado", "A");
        var cursoB = await SembrarCursoAsync(contexto, anio.Id, "1er Grado", "B");
        contexto.AsignacionesDocentes.AddRange(
            new AsignacionDocente
            {
                UsuarioId = 2,
                MateriaId = matematica.Id,
                CursoId = cursoA.Id,
                HorasSemanales = 4,
            },
            new AsignacionDocente
            {
                UsuarioId = 3,
                MateriaId = programacion.Id,
                CursoId = cursoB.Id,
                HorasSemanales = 6,
            });
        await contexto.SaveChangesAsync();
        var consulta = new ConsultaUsuariosFalsa(
            Docente(2, "Ana Docente"),
            Docente(3, "Luis Docente"));
        var servicio = new AsignacionesServicio(contexto, consulta);

        // Actuar
        var asignaciones = await servicio.ListarAsync(cursoA.Id, null);

        // Verificar
        var asignacion = Assert.Single(asignaciones);
        Assert.Equal(cursoA.Id, asignacion.CursoId);
        Assert.Equal("Ana Docente", asignacion.DocenteNombre);
        Assert.Equal("docente2@ejemplo.com", asignacion.DocenteCorreo);
        Assert.Equal("Matemática", asignacion.MateriaNombre);
        Assert.Equal("1er Grado", asignacion.Grado);
        Assert.Equal("A", asignacion.Seccion);
        Assert.Equal("2026-2027", asignacion.AnioEscolarNombre);
        Assert.Equal(4, asignacion.HorasSemanales);
        Assert.Equal(1, consulta.LlamadasObtenerVarios);
    }

    private static TamsDbContext CrearContexto()
    {
        var opciones = new DbContextOptionsBuilder<TamsDbContext>()
            .UseInMemoryDatabase($"Asignaciones-{Guid.NewGuid():N}")
            .Options;

        return new TamsDbContext(opciones);
    }

    private static async Task<AnioEscolar> SembrarAnioAsync(TamsDbContext contexto)
    {
        var anio = new AnioEscolar
        {
            Nombre = "2026-2027",
            FechaInicio = new DateOnly(2026, 8, 1),
            FechaFin = new DateOnly(2027, 6, 30),
            EsActivo = true,
        };

        contexto.AniosEscolares.Add(anio);
        await contexto.SaveChangesAsync();

        return anio;
    }

    private static async Task<Materia> SembrarMateriaAsync(
        TamsDbContext contexto,
        string nombre,
        bool esTecnica = false,
        TipoTecnico? tipoTecnico = null)
    {
        var materia = new Materia
        {
            Nombre = nombre,
            EsTecnica = esTecnica,
            TipoTecnico = tipoTecnico,
        };

        contexto.Materias.Add(materia);
        await contexto.SaveChangesAsync();

        return materia;
    }

    private static async Task<Curso> SembrarCursoAsync(
        TamsDbContext contexto,
        int anioEscolarId,
        string grado,
        string seccion,
        bool esTecnico = false,
        TipoTecnico? tipoTecnico = null)
    {
        var curso = new Curso
        {
            Grado = grado,
            Seccion = seccion,
            EsTecnico = esTecnico,
            TipoTecnico = tipoTecnico,
            AnioEscolarId = anioEscolarId,
        };

        contexto.Cursos.Add(curso);
        await contexto.SaveChangesAsync();

        return curso;
    }

    private static UsuarioConsultaResumen Docente(
        int id,
        string nombre,
        bool activo = true)
        => Usuario(id, nombre, "Estandar", activo);

    private static UsuarioConsultaResumen Usuario(
        int id,
        string nombre,
        string rol,
        bool activo = true)
        => new(id, nombre, $"docente{id}@ejemplo.com", rol, activo);

    private sealed class ConsultaUsuariosFalsa : IConsultaUsuariosServicio
    {
        private readonly Dictionary<int, UsuarioConsultaResumen> _usuarios;

        public ConsultaUsuariosFalsa(params UsuarioConsultaResumen[] usuarios)
        {
            _usuarios = usuarios.ToDictionary(usuario => usuario.Id);
        }

        public int LlamadasObtenerVarios { get; private set; }

        public Task<UsuarioConsultaResumen?> ObtenerAsync(
            int usuarioId,
            CancellationToken cancellationToken = default)
            => Task.FromResult(_usuarios.GetValueOrDefault(usuarioId));

        public Task<IReadOnlyList<UsuarioConsultaResumen>> ObtenerVariosAsync(
            IReadOnlyCollection<int> usuarioIds,
            CancellationToken cancellationToken = default)
        {
            LlamadasObtenerVarios++;

            var encontrados = usuarioIds
                .Where(id => _usuarios.ContainsKey(id))
                .Select(id => _usuarios[id])
                .OrderBy(usuario => usuario.Id)
                .ToList();

            return Task.FromResult<IReadOnlyList<UsuarioConsultaResumen>>(encontrados);
        }
    }
}
