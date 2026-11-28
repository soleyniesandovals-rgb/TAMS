using Core.ControlAcceso.Domain.Excepciones;
using Core.Permisos.Application.Interfaces;
using Core.Permisos.Application.Modelos;
using Microsoft.EntityFrameworkCore;
using Tams.Negocio.Application.Servicios;
using Tams.Negocio.Domain;
using Tams.Negocio.Domain.Entidades;
using Tams.Negocio.Domain.Enums;
using Tams.Negocio.Infrastructura.Persistence;
using Xunit;

namespace Tests.Negocio;

/// <summary>
/// RF-NEG-02 (RD-02, RD-07): el docente titular puede cambiar el puntaje mientras la
/// calificación está en Borrador, sin permiso y sin transición de estado (RD-04).
/// </summary>
public class RF_NEG_EditarBorradorTests
{
    private const int DocenteId = 10;
    private const int OtroDocenteId = 11;
    private const int EstudianteId = 1;
    private const int MateriaBasicaId = 1;
    private const int MateriaTecnicaId = 2;
    private const int AnioId = 1;
    private const string MensajeRango = "El puntaje debe estar entre 0 y 100.";
    private const string MensajeEstado = "Solo se puede cambiar el puntaje de una calificación en Borrador.";

    [Theory]
    [InlineData(EstadoCalificacion.Borrador, true)]
    [InlineData(EstadoCalificacion.Enviada, false)]
    [InlineData(EstadoCalificacion.Publicada, false)]
    [InlineData(EstadoCalificacion.EnRecuperacion, false)]
    [InlineData(EstadoCalificacion.Cerrada, false)]
    public void RF_NEG_EsEditable_SoloBorrador(EstadoCalificacion estado, bool esperado)
    {
        // Preparar: el estado llega como dato de la teoría.

        // Actuar: se evalúa la regla pura del dominio.
        var resultado = ReglasCalificacion.EsEditable(estado);

        // Verificar: solo Borrador admite el cambio de puntaje; la máquina no cambia.
        Assert.Equal(esperado, resultado);
    }

    [Fact]
    public async Task RF_NEG_EditarBorrador_BorradorDePeriodo_CambiaPuntajeYConservaEstado()
    {
        // Preparar: una calificación de periodo en Borrador con 80 puntos.
        await using var contexto = await CrearContextoAsync();
        var nota = await SembrarNotaAsync(
            contexto, TipoEvaluacion.Periodo, 1, EstadoCalificacion.Borrador, 80m);
        var servicio = CrearServicio(contexto);

        // Actuar: se corrige el puntaje del borrador.
        var resultado = await servicio.EditarBorradorAsync(DocenteId, nota.Id, 92m);

        // Verificar: se guarda el nuevo puntaje, sigue en Borrador y no hay advertencia.
        Assert.Equal(92m, resultado.PuntajeObtenido);
        Assert.Equal("Borrador", resultado.Estado);
        Assert.Null(resultado.Advertencia);
        Assert.Equal(92m, await ObtenerPuntajeAsync(contexto, nota.Id));
        Assert.Equal(EstadoCalificacion.Borrador, await ObtenerEstadoAsync(contexto, nota.Id));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    public async Task RF_NEG_EditarBorrador_PuntajeEnLimite_SeAcepta(int puntaje)
    {
        // Preparar: un borrador de periodo con 80 puntos.
        await using var contexto = await CrearContextoAsync();
        var nota = await SembrarNotaAsync(
            contexto, TipoEvaluacion.Periodo, 1, EstadoCalificacion.Borrador, 80m);
        var servicio = CrearServicio(contexto);

        // Actuar: se edita con el extremo inferior o el superior de la escala.
        var resultado = await servicio.EditarBorradorAsync(DocenteId, nota.Id, puntaje);

        // Verificar: ambos extremos son válidos y se persisten.
        Assert.Equal(puntaje, resultado.PuntajeObtenido);
        Assert.Equal(puntaje, await ObtenerPuntajeAsync(contexto, nota.Id));
        Assert.Equal("Borrador", resultado.Estado);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(100.01)]
    public async Task RF_NEG_EditarBorrador_PuntajeFueraDeRango_Rechaza400YNoCambia(decimal puntaje)
    {
        // Preparar: un borrador de periodo con 80 puntos.
        await using var contexto = await CrearContextoAsync();
        var nota = await SembrarNotaAsync(
            contexto, TipoEvaluacion.Periodo, 1, EstadoCalificacion.Borrador, 80m);
        var servicio = CrearServicio(contexto);

        // Actuar: se intenta editar con un puntaje fuera de 0..100.
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() =>
            servicio.EditarBorradorAsync(DocenteId, nota.Id, puntaje));

        // Verificar: 400 controlado, mensaje exacto y puntaje sin cambiar.
        Assert.Equal(400, excepcion.CodigoHttp);
        Assert.Equal(MensajeRango, excepcion.Message);
        Assert.Equal(80m, await ObtenerPuntajeAsync(contexto, nota.Id));
        Assert.Equal(EstadoCalificacion.Borrador, await ObtenerEstadoAsync(contexto, nota.Id));
    }

    [Theory]
    [InlineData(EstadoCalificacion.Enviada)]
    [InlineData(EstadoCalificacion.Publicada)]
    [InlineData(EstadoCalificacion.EnRecuperacion)]
    [InlineData(EstadoCalificacion.Cerrada)]
    public async Task RF_NEG_EditarBorrador_EstadoDistintoDeBorrador_Rechaza409YNoCambia(
        EstadoCalificacion estado)
    {
        // Preparar: una calificación en cualquiera de los estados que no son Borrador.
        await using var contexto = await CrearContextoAsync();
        var nota = await SembrarNotaAsync(
            contexto, TipoEvaluacion.Periodo, 1, estado, 80m);
        var servicio = CrearServicio(contexto);

        // Actuar: se intenta editar el puntaje.
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() =>
            servicio.EditarBorradorAsync(DocenteId, nota.Id, 92m));

        // Verificar: 409 controlado, mensaje exacto y calificación intacta.
        Assert.Equal(409, excepcion.CodigoHttp);
        Assert.Equal(MensajeEstado, excepcion.Message);
        Assert.Equal(80m, await ObtenerPuntajeAsync(contexto, nota.Id));
        Assert.Equal(estado, await ObtenerEstadoAsync(contexto, nota.Id));
    }

    [Fact]
    public async Task RF_NEG_EditarBorrador_OtroDocente_Rechaza403YNoCambia()
    {
        // Preparar: un borrador cuyo titular es otro docente.
        await using var contexto = await CrearContextoAsync();
        var nota = await SembrarNotaAsync(
            contexto, TipoEvaluacion.Periodo, 1, EstadoCalificacion.Borrador, 80m);
        var servicio = CrearServicio(contexto);

        // Actuar: intenta editarlo un docente sin asignación en ese curso y materia.
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() =>
            servicio.EditarBorradorAsync(OtroDocenteId, nota.Id, 92m));

        // Verificar: 403 controlado, mensaje exacto y puntaje sin cambiar.
        Assert.Equal(403, excepcion.CodigoHttp);
        Assert.Equal("No eres el docente titular de esta calificación.", excepcion.Message);
        Assert.Equal(80m, await ObtenerPuntajeAsync(contexto, nota.Id));
        Assert.Equal(EstadoCalificacion.Borrador, await ObtenerEstadoAsync(contexto, nota.Id));
    }

    [Fact]
    public async Task RF_NEG_EditarBorrador_CalificacionInexistente_Rechaza404()
    {
        // Preparar: no hay ninguna calificación guardada.
        await using var contexto = await CrearContextoAsync();
        var servicio = CrearServicio(contexto);

        // Actuar: se intenta editar un id que no existe.
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() =>
            servicio.EditarBorradorAsync(DocenteId, 999, 92m));

        // Verificar: 404 controlado con el mensaje exacto.
        Assert.Equal(404, excepcion.CodigoHttp);
        Assert.Equal("La calificación no existe.", excepcion.Message);
        Assert.Equal(0, await contexto.Calificaciones.CountAsync());
    }

    [Fact]
    public async Task RF_NEG_EditarBorrador_RaPorDebajoDelMinimo_SeGuardaConAdvertencia()
    {
        // Preparar: un borrador de RA1 con mínimo 70 y un doble de permisos que devuelve false.
        await using var contexto = await CrearContextoAsync();
        await SembrarRaAsync(contexto, 1, 70);
        var nota = await SembrarNotaAsync(
            contexto, TipoEvaluacion.RA, 1, EstadoCalificacion.Borrador, 80m, MateriaTecnicaId);
        var servicio = CrearServicio(contexto);

        // Actuar: se baja la nota del RA por debajo del mínimo.
        var resultado = await servicio.EditarBorradorAsync(DocenteId, nota.Id, 60m);

        // Verificar: se guarda con la advertencia exacta, igual que Registrar y Corregir.
        Assert.Equal("La nota del RA1 está por debajo del mínimo de 70 puntos.", resultado.Advertencia);
        Assert.Equal(60m, resultado.PuntajeObtenido);
        Assert.Equal("Borrador", resultado.Estado);
        Assert.Equal(60m, await ObtenerPuntajeAsync(contexto, nota.Id));
    }

    [Theory]
    [InlineData(70)]
    [InlineData(85)]
    public async Task RF_NEG_EditarBorrador_RaEnOMasDelMinimo_SinAdvertencia(int puntaje)
    {
        // Preparar: un borrador de RA1 con mínimo 70.
        await using var contexto = await CrearContextoAsync();
        await SembrarRaAsync(contexto, 1, 70);
        var nota = await SembrarNotaAsync(
            contexto, TipoEvaluacion.RA, 1, EstadoCalificacion.Borrador, 60m, MateriaTecnicaId);
        var servicio = CrearServicio(contexto);

        // Actuar: se edita con el mínimo exacto y por encima de él.
        var resultado = await servicio.EditarBorradorAsync(DocenteId, nota.Id, puntaje);

        // Verificar: no hay advertencia y la nota se guarda igual.
        Assert.Null(resultado.Advertencia);
        Assert.Equal(puntaje, resultado.PuntajeObtenido);
        Assert.Equal(puntaje, await ObtenerPuntajeAsync(contexto, nota.Id));
    }

    [Fact]
    public async Task RF_NEG_EditarBorrador_NoConsultaPermiso_PermisoFalsoNoImpide()
    {
        // Preparar: un borrador y un doble que devuelve false y cuenta las consultas.
        await using var contexto = await CrearContextoAsync();
        var nota = await SembrarNotaAsync(
            contexto, TipoEvaluacion.Periodo, 1, EstadoCalificacion.Borrador, 80m);
        var permisos = new GestionPermisosFalso();
        var servicio = CrearServicio(contexto, permisos);

        // Actuar: se edita el borrador sin permiso de corrección.
        var resultado = await servicio.EditarBorradorAsync(DocenteId, nota.Id, 95m);

        // Verificar: la edición se hace y el permiso ni se consulta.
        Assert.Equal(95m, resultado.PuntajeObtenido);
        Assert.Equal(0, permisos.Consultas);
        Assert.Equal(95m, await ObtenerPuntajeAsync(contexto, nota.Id));
    }

    private static CalificacionesServicio CrearServicio(
        TamsDbContext contexto,
        GestionPermisosFalso? permisos = null)
        => new(contexto, permisos ?? new GestionPermisosFalso());

    private static async Task<decimal> ObtenerPuntajeAsync(TamsDbContext contexto, int id)
        => (await contexto.Calificaciones
                .AsNoTracking()
                .SingleAsync(c => c.Id == id))
            .PuntajeObtenido;

    private static async Task<EstadoCalificacion> ObtenerEstadoAsync(TamsDbContext contexto, int id)
        => (await contexto.Calificaciones
                .AsNoTracking()
                .SingleAsync(c => c.Id == id))
            .Estado;

    private static async Task<TamsDbContext> CrearContextoAsync()
    {
        var opciones = new DbContextOptionsBuilder<TamsDbContext>()
            .UseInMemoryDatabase($"EditarBorrador-{Guid.NewGuid():N}")
            .Options;

        var contexto = new TamsDbContext(opciones);

        var anio = new AnioEscolar
        {
            Id = AnioId,
            Nombre = "2026-2027",
            FechaInicio = new DateOnly(2026, 8, 1),
            FechaFin = new DateOnly(2027, 6, 30),
            EsActivo = true,
        };
        var curso = new Curso
        {
            Id = 1,
            Grado = "1er Grado",
            Seccion = "A",
            AnioEscolarId = anio.Id,
            AnioEscolar = anio,
        };
        var materiaBasica = new Materia { Id = MateriaBasicaId, Nombre = "Matemática", EsTecnica = false };
        var materiaTecnica = new Materia
        {
            Id = MateriaTecnicaId,
            Nombre = "Programación",
            EsTecnica = true,
            CantidadRA = 3,
        };
        var estudiante = new Estudiante
        {
            Id = EstudianteId,
            Nombre = "Ana",
            Apellido = "Pérez",
            CursoId = curso.Id,
            Curso = curso,
        };
        var asignacionBasica = new AsignacionDocente
        {
            Id = 1,
            UsuarioId = DocenteId,
            MateriaId = materiaBasica.Id,
            CursoId = curso.Id,
            HorasSemanales = 4,
            Materia = materiaBasica,
            Curso = curso,
        };
        var asignacionTecnica = new AsignacionDocente
        {
            Id = 2,
            UsuarioId = DocenteId,
            MateriaId = materiaTecnica.Id,
            CursoId = curso.Id,
            HorasSemanales = 3,
            Materia = materiaTecnica,
            Curso = curso,
        };

        contexto.AniosEscolares.Add(anio);
        contexto.Cursos.Add(curso);
        contexto.Materias.AddRange(materiaBasica, materiaTecnica);
        contexto.Estudiantes.Add(estudiante);
        contexto.AsignacionesDocentes.AddRange(asignacionBasica, asignacionTecnica);
        await contexto.SaveChangesAsync();

        return contexto;
    }

    private static async Task SembrarRaAsync(TamsDbContext contexto, int numero, int? puntajeMinimo)
    {
        contexto.ResultadosAprendizaje.Add(new ResultadoAprendizaje
        {
            MateriaId = MateriaTecnicaId,
            Numero = numero,
            Puntos = 100,
            PuntajeMinimo = puntajeMinimo,
        });

        await contexto.SaveChangesAsync();
    }

    private static async Task<Calificacion> SembrarNotaAsync(
        TamsDbContext contexto,
        TipoEvaluacion tipo,
        int numero,
        EstadoCalificacion estado,
        decimal puntaje = 80m,
        int? materiaId = null)
    {
        var nota = new Calificacion
        {
            EstudianteId = EstudianteId,
            MateriaId = materiaId ?? MateriaBasicaId,
            AnioEscolarId = AnioId,
            TipoEvaluacion = tipo,
            Periodo = tipo == TipoEvaluacion.Periodo ? numero : null,
            NumeroRA = tipo == TipoEvaluacion.RA ? numero : null,
            PuntajeObtenido = puntaje,
            Estado = estado,
        };

        contexto.Calificaciones.Add(nota);
        await contexto.SaveChangesAsync();

        return nota;
    }

    /// <summary>Doble en memoria: editar borrador no consulta permisos (RF-NEG-02, RD-12).</summary>
    private sealed class GestionPermisosFalso : IGestionPermisosServicio
    {
        public int Consultas { get; private set; }

        public Task<SolicitudPermisoResumen> CrearAsync(
            int solicitanteId, string tipoPermiso, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<IReadOnlyList<SolicitudPermisoResumen>> ListarPropiasAsync(
            int usuarioId, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<IReadOnlyList<SolicitudPermisoResumen>> ListarPendientesAsync(
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<SolicitudPermisoResumen> AprobarAsync(
            int solicitudId, int administradorId, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<SolicitudPermisoResumen> RechazarAsync(
            int solicitudId, int administradorId, string motivo, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<bool> TienePermisoAsync(
            int usuarioId, string tipoPermiso, CancellationToken cancellationToken = default)
        {
            Consultas++;
            return Task.FromResult(false);
        }
    }
}
