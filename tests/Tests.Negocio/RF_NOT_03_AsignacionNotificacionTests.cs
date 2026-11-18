using Core.ControlAcceso.Application.Interfaces;
using Core.ControlAcceso.Application.Modelos;
using Core.Notificaciones.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Tams.Negocio.Application.Servicios;
using Tams.Negocio.Domain.Entidades;
using Tams.Negocio.Infrastructura.Persistence;
using Tests.Negocio.Dobles;
using Xunit;

namespace Tests.Negocio;

/// <summary>
/// C-05 (RF-NOT-03): evento al asignar docente y tolerancia a fallo.
/// </summary>
public class AsignacionNotificacionTests
{
    [Fact]
    public async Task RF_NOT_03_CrearAsignacion_NotificaAlDocenteConMateriaCursoYHoras()
    {
        // Preparar: catálogo mínimo, docente Estándar activo y notificador de captura.
        await using var contexto = CrearContexto();
        var anio = await SembrarAnioAsync(contexto);
        var materia = await SembrarMateriaAsync(contexto, "Matemática");
        var curso = await SembrarCursoAsync(contexto, anio.Id, "1er Grado", "A");
        var consulta = new ConsultaUsuariosFalsa(Docente(2, "Ana Docente"));
        var notificador = new NotificadorFalso();
        var servicio = new AsignacionesServicio(
            contexto, consulta, notificador, NullLogger<AsignacionesServicio>.Instance);

        // Actuar: se crea la asignación.
        var resumen = await servicio.CrearAsync(2, materia.Id, curso.Id, 5);

        // Verificar: la asignación se guardó y se avisó una vez al docente con los datos del curso.
        Assert.True(resumen.Id > 0);
        Assert.Single(await contexto.AsignacionesDocentes.AsNoTracking().ToListAsync());
        var llamada = Assert.Single(notificador.Llamadas);
        Assert.Equal(2, llamada.UsuarioId);
        Assert.Equal(TipoNotificacion.AsignacionCreada, llamada.Tipo);
        Assert.Contains(materia.Nombre, llamada.Mensaje, StringComparison.Ordinal);
        Assert.Contains(curso.Grado, llamada.Mensaje, StringComparison.Ordinal);
        Assert.Contains(curso.Seccion, llamada.Mensaje, StringComparison.Ordinal);
        Assert.Contains(anio.Nombre, llamada.Mensaje, StringComparison.Ordinal);
        Assert.Contains("5", llamada.Mensaje, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RF_NOT_03_FallaNotificacion_AsignacionSeConservaYNoPropagaError()
    {
        // Preparar: catálogo mínimo y un notificador que siempre falla.
        await using var contexto = CrearContexto();
        var anio = await SembrarAnioAsync(contexto);
        var materia = await SembrarMateriaAsync(contexto, "Lengua");
        var curso = await SembrarCursoAsync(contexto, anio.Id, "2do Grado", "B");
        var consulta = new ConsultaUsuariosFalsa(Docente(3, "Luis Docente"));
        var servicio = new AsignacionesServicio(
            contexto,
            consulta,
            new NotificadorQueFalla(),
            NullLogger<AsignacionesServicio>.Instance);

        // Actuar: se crea la asignación aunque el aviso falle.
        var resumen = await servicio.CrearAsync(3, materia.Id, curso.Id, 4);

        // Verificar: la operación terminó bien y la asignación sigue guardada.
        Assert.True(resumen.Id > 0);
        Assert.Equal(3, resumen.UsuarioId);
        var guardada = await contexto.AsignacionesDocentes.AsNoTracking().SingleAsync();
        Assert.Equal(3, guardada.UsuarioId);
        Assert.Equal(materia.Id, guardada.MateriaId);
        Assert.Equal(curso.Id, guardada.CursoId);
    }

    private static TamsDbContext CrearContexto()
    {
        var opciones = new DbContextOptionsBuilder<TamsDbContext>()
            .UseInMemoryDatabase($"AsignacionesNotificacion-{Guid.NewGuid():N}")
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

    private static async Task<Materia> SembrarMateriaAsync(TamsDbContext contexto, string nombre)
    {
        var materia = new Materia { Nombre = nombre };

        contexto.Materias.Add(materia);
        await contexto.SaveChangesAsync();

        return materia;
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
            AnioEscolarId = anioEscolarId,
        };

        contexto.Cursos.Add(curso);
        await contexto.SaveChangesAsync();

        return curso;
    }

    private static UsuarioConsultaResumen Docente(int id, string nombre)
        => new(id, nombre, $"docente{id}@ejemplo.com", "Estandar", true);

    private sealed class ConsultaUsuariosFalsa : IConsultaUsuariosServicio
    {
        private readonly Dictionary<int, UsuarioConsultaResumen> _usuarios;

        public ConsultaUsuariosFalsa(params UsuarioConsultaResumen[] usuarios)
        {
            _usuarios = usuarios.ToDictionary(usuario => usuario.Id);
        }

        public Task<UsuarioConsultaResumen?> ObtenerAsync(
            int usuarioId,
            CancellationToken cancellationToken = default)
            => Task.FromResult(_usuarios.GetValueOrDefault(usuarioId));

        public Task<IReadOnlyList<UsuarioConsultaResumen>> ObtenerVariosAsync(
            IReadOnlyCollection<int> usuarioIds,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<UsuarioConsultaResumen>>(
                usuarioIds.Where(_usuarios.ContainsKey).Select(id => _usuarios[id]).ToList());
    }
}
