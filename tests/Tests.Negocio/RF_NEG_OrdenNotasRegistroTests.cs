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
/// RF-NEG (RD-02): al registrar, la nota n (n &gt;= 2) exige que la anterior esté publicada.
/// La regla se comprueba en el servicio con EF Core InMemory; no toca las transiciones.
/// </summary>
public class RF_NEG_OrdenNotasRegistroTests
{
    private const int DocenteId = 10;
    private const int EstudianteId = 1;
    private const int OtroEstudianteId = 2;
    private const int MateriaBasicaId = 1;
    private const int MateriaTecnicaId = 2;
    private const int AnioId = 1;
    private const int OtroAnioId = 2;

    [Fact]
    public async Task RF_NEG_Registrar_P1_SinNotaAnterior_SeRegistra()
    {
        // Preparar: no hay ninguna calificación registrada.
        await using var contexto = await CrearContextoAsync();
        var servicio = CrearServicio(contexto);

        // Actuar: el periodo 1 no necesita nota previa.
        var resultado = await servicio.RegistrarAsync(
            DocenteId, EstudianteId, MateriaBasicaId, AnioId,
            TipoEvaluacion.Periodo, 1, null, 85m);

        // Verificar: se guarda en Borrador.
        Assert.Equal("Borrador", resultado.Estado);
        Assert.Equal(1, resultado.Periodo);
        Assert.Equal(1, await contexto.Calificaciones.CountAsync());
    }

    [Fact]
    public async Task RF_NEG_Registrar_P2_SinP1_Rechaza409YNoGuarda()
    {
        // Preparar: no hay P1.
        await using var contexto = await CrearContextoAsync();
        var servicio = CrearServicio(contexto);

        // Actuar: se intenta registrar el P2.
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() =>
            servicio.RegistrarAsync(
                DocenteId, EstudianteId, MateriaBasicaId, AnioId,
                TipoEvaluacion.Periodo, 2, null, 85m));

        // Verificar: 409 controlado y ninguna nota guardada.
        Assert.Equal(409, excepcion.CodigoHttp);
        Assert.Equal("Primero debe estar publicada la nota del P1.", excepcion.Message);
        Assert.Equal(0, await contexto.Calificaciones.CountAsync());
    }

    [Fact]
    public async Task RF_NEG_Registrar_P2_ConP1EnBorrador_Rechaza409YNoGuarda()
    {
        // Preparar: el P1 existe pero sigue en Borrador.
        await using var contexto = await CrearContextoAsync();
        await SembrarNotaAsync(contexto, TipoEvaluacion.Periodo, 1, EstadoCalificacion.Borrador);
        var servicio = CrearServicio(contexto);

        // Actuar: se intenta registrar el P2.
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() =>
            servicio.RegistrarAsync(
                DocenteId, EstudianteId, MateriaBasicaId, AnioId,
                TipoEvaluacion.Periodo, 2, null, 85m));

        // Verificar: Borrador no habilita y no se guarda el P2.
        Assert.Equal(409, excepcion.CodigoHttp);
        Assert.Equal("Primero debe estar publicada la nota del P1.", excepcion.Message);
        Assert.Equal(1, await contexto.Calificaciones.CountAsync());
    }

    [Fact]
    public async Task RF_NEG_Registrar_P2_ConP1Enviada_Rechaza409YNoGuarda()
    {
        // Preparar: el P1 existe pero sigue en Enviada.
        await using var contexto = await CrearContextoAsync();
        await SembrarNotaAsync(contexto, TipoEvaluacion.Periodo, 1, EstadoCalificacion.Enviada);
        var servicio = CrearServicio(contexto);

        // Actuar: se intenta registrar el P2.
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() =>
            servicio.RegistrarAsync(
                DocenteId, EstudianteId, MateriaBasicaId, AnioId,
                TipoEvaluacion.Periodo, 2, null, 85m));

        // Verificar: Enviada no habilita y no se guarda el P2.
        Assert.Equal(409, excepcion.CodigoHttp);
        Assert.Equal("Primero debe estar publicada la nota del P1.", excepcion.Message);
        Assert.Equal(1, await contexto.Calificaciones.CountAsync());
    }

    [Fact]
    public async Task RF_NEG_Registrar_P2_ConP1Publicada_SeRegistra()
    {
        // Preparar: el P1 ya está Publicada.
        await using var contexto = await CrearContextoAsync();
        await SembrarNotaAsync(contexto, TipoEvaluacion.Periodo, 1, EstadoCalificacion.Publicada);
        var servicio = CrearServicio(contexto);

        // Actuar: se registra el P2.
        var resultado = await servicio.RegistrarAsync(
            DocenteId, EstudianteId, MateriaBasicaId, AnioId,
            TipoEvaluacion.Periodo, 2, null, 85m);

        // Verificar: el P2 se guarda en Borrador junto al P1.
        Assert.Equal("Borrador", resultado.Estado);
        Assert.Equal(2, resultado.Periodo);
        Assert.Equal(2, await contexto.Calificaciones.CountAsync());
    }

    [Fact]
    public async Task RF_NEG_Registrar_P2_ConP1EnRecuperacion_SeRegistra()
    {
        // Preparar: EnRecuperacion también pasó por la publicación.
        await using var contexto = await CrearContextoAsync();
        await SembrarNotaAsync(
            contexto, TipoEvaluacion.Periodo, 1, EstadoCalificacion.EnRecuperacion, 60m);
        var servicio = CrearServicio(contexto);

        // Actuar: se registra el P2.
        var resultado = await servicio.RegistrarAsync(
            DocenteId, EstudianteId, MateriaBasicaId, AnioId,
            TipoEvaluacion.Periodo, 2, null, 85m);

        // Verificar: el P2 se guarda.
        Assert.Equal("Borrador", resultado.Estado);
        Assert.Equal(2, await contexto.Calificaciones.CountAsync());
    }

    [Fact]
    public async Task RF_NEG_Registrar_P2_ConP1Cerrada_SeRegistra()
    {
        // Preparar: Cerrada también pasó por la publicación.
        await using var contexto = await CrearContextoAsync();
        await SembrarNotaAsync(contexto, TipoEvaluacion.Periodo, 1, EstadoCalificacion.Cerrada);
        var servicio = CrearServicio(contexto);

        // Actuar: se registra el P2.
        var resultado = await servicio.RegistrarAsync(
            DocenteId, EstudianteId, MateriaBasicaId, AnioId,
            TipoEvaluacion.Periodo, 2, null, 85m);

        // Verificar: el P2 se guarda.
        Assert.Equal("Borrador", resultado.Estado);
        Assert.Equal(2, await contexto.Calificaciones.CountAsync());
    }

    [Fact]
    public async Task RF_NEG_Registrar_RA2_SinRA1Publicada_Rechaza409YNoGuarda()
    {
        // Preparar: no hay RA1.
        await using var contexto = await CrearContextoAsync();
        var servicio = CrearServicio(contexto);

        // Actuar: se intenta registrar el RA2.
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() =>
            servicio.RegistrarAsync(
                DocenteId, EstudianteId, MateriaTecnicaId, AnioId,
                TipoEvaluacion.RA, null, 2, 85m));

        // Verificar: mensaje propio de RA y ninguna nota guardada.
        Assert.Equal(409, excepcion.CodigoHttp);
        Assert.Equal("Primero debe estar publicada la nota del RA1.", excepcion.Message);
        Assert.Equal(0, await contexto.Calificaciones.CountAsync());
    }

    [Fact]
    public async Task RF_NEG_Registrar_RA2_ConRA1Publicada_SeRegistra()
    {
        // Preparar: el RA1 ya está Publicada en la materia técnica.
        await using var contexto = await CrearContextoAsync();
        await SembrarNotaAsync(
            contexto, TipoEvaluacion.RA, 1, EstadoCalificacion.Publicada,
            materiaId: MateriaTecnicaId);
        var servicio = CrearServicio(contexto);

        // Actuar: se registra el RA2.
        var resultado = await servicio.RegistrarAsync(
            DocenteId, EstudianteId, MateriaTecnicaId, AnioId,
            TipoEvaluacion.RA, null, 2, 85m);

        // Verificar: el RA2 se guarda en Borrador.
        Assert.Equal("Borrador", resultado.Estado);
        Assert.Equal(2, resultado.NumeroRA);
        Assert.Equal(2, await contexto.Calificaciones.CountAsync());
    }

    public static TheoryData<int, int, int> ContextosAjenos => new()
    {
        { OtroEstudianteId, MateriaBasicaId, AnioId },
        { EstudianteId, MateriaTecnicaId, AnioId },
        { EstudianteId, MateriaBasicaId, OtroAnioId },
    };

    [Theory]
    [MemberData(nameof(ContextosAjenos))]
    public async Task RF_NEG_Registrar_P2_ConP1PublicadoDeOtroContexto_Rechaza409YNoGuarda(
        int estudianteAnteriorId,
        int materiaAnteriorId,
        int anioAnteriorId)
    {
        // Preparar: un P1 publicado que pertenece a otro estudiante, materia o año.
        await using var contexto = await CrearContextoAsync();
        await SembrarNotaAsync(
            contexto, TipoEvaluacion.Periodo, 1, EstadoCalificacion.Publicada,
            estudianteId: estudianteAnteriorId,
            materiaId: materiaAnteriorId,
            anioEscolarId: anioAnteriorId);
        var servicio = CrearServicio(contexto);

        // Actuar: se intenta registrar el P2 del estudiante, materia y año canónicos.
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() =>
            servicio.RegistrarAsync(
                DocenteId, EstudianteId, MateriaBasicaId, AnioId,
                TipoEvaluacion.Periodo, 2, null, 85m));

        // Verificar: la nota ajena no habilita y no se guarda nada.
        Assert.Equal(409, excepcion.CodigoHttp);
        Assert.Equal("Primero debe estar publicada la nota del P1.", excepcion.Message);
        Assert.Equal(1, await contexto.Calificaciones.CountAsync());
    }

    [Theory]
    [InlineData(EstadoCalificacion.Borrador, false)]
    [InlineData(EstadoCalificacion.Enviada, false)]
    [InlineData(EstadoCalificacion.Publicada, true)]
    [InlineData(EstadoCalificacion.EnRecuperacion, true)]
    [InlineData(EstadoCalificacion.Cerrada, true)]
    public void RF_NEG_EstaPublicada_SoloEstadosQuePasaronPorPublicacion(
        EstadoCalificacion estado,
        bool esperado)
    {
        // Preparar: el estado bajo prueba llega como dato de la teoría.

        // Actuar: se evalúa la regla pura del dominio.
        var resultado = ReglasCalificacion.EstaPublicada(estado);

        // Verificar: solo Publicada, EnRecuperacion y Cerrada cuentan como publicadas.
        Assert.Equal(esperado, resultado);
    }

    private static CalificacionesServicio CrearServicio(TamsDbContext contexto)
        => new(contexto, new GestionPermisosFalso());

    private static async Task<TamsDbContext> CrearContextoAsync()
    {
        var opciones = new DbContextOptionsBuilder<TamsDbContext>()
            .UseInMemoryDatabase($"OrdenNotas-{Guid.NewGuid():N}")
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
        var otroAnio = new AnioEscolar
        {
            Id = OtroAnioId,
            Nombre = "2025-2026",
            FechaInicio = new DateOnly(2025, 8, 1),
            FechaFin = new DateOnly(2026, 6, 30),
            EsActivo = false,
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
        var otroEstudiante = new Estudiante
        {
            Id = OtroEstudianteId,
            Nombre = "Luis",
            Apellido = "Gómez",
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

        contexto.AniosEscolares.AddRange(anio, otroAnio);
        contexto.Cursos.Add(curso);
        contexto.Materias.AddRange(materiaBasica, materiaTecnica);
        contexto.Estudiantes.AddRange(estudiante, otroEstudiante);
        contexto.AsignacionesDocentes.AddRange(asignacionBasica, asignacionTecnica);
        await contexto.SaveChangesAsync();

        return contexto;
    }

    private static async Task SembrarNotaAsync(
        TamsDbContext contexto,
        TipoEvaluacion tipo,
        int numero,
        EstadoCalificacion estado,
        decimal puntaje = 80m,
        int? estudianteId = null,
        int? materiaId = null,
        int? anioEscolarId = null)
    {
        contexto.Calificaciones.Add(new Calificacion
        {
            EstudianteId = estudianteId ?? EstudianteId,
            MateriaId = materiaId ?? MateriaBasicaId,
            AnioEscolarId = anioEscolarId ?? AnioId,
            TipoEvaluacion = tipo,
            Periodo = tipo == TipoEvaluacion.Periodo ? numero : null,
            NumeroRA = tipo == TipoEvaluacion.RA ? numero : null,
            PuntajeObtenido = puntaje,
            Estado = estado,
        });

        await contexto.SaveChangesAsync();
    }

    /// <summary>Doble en memoria: registrar no consulta permisos (RD-12).</summary>
    private sealed class GestionPermisosFalso : IGestionPermisosServicio
    {
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
            => Task.FromResult(false);
    }
}
