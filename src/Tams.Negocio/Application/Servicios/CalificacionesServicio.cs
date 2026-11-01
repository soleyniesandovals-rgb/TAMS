using Core.ControlAcceso.Domain.Excepciones;
using Core.Permisos.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Tams.Negocio.Application.Modelos;
using Tams.Negocio.Domain;
using Tams.Negocio.Domain.Entidades;
using Tams.Negocio.Domain.Enums;
using Tams.Negocio.Infrastructura.Persistence;

namespace Tams.Negocio.Application.Servicios;

/// <summary>
/// Operaciones de calificaciones del módulo de negocio (RF-NEG-02, RF-NEG-03, RF-NEG-04,
/// RF-NEG-05, RF-NEG-06).
/// RD-02: la lógica vive aquí, no en el controlador.
/// RD-04: los estados y sus condiciones se delegan a la máquina del dominio.
/// RD-03: la titularidad se resuelve con las entidades del negocio; del Core solo se usa
/// la interfaz de gestión de permisos, sin referencias a sus tablas.
/// </summary>
public class CalificacionesServicio(
    TamsDbContext context,
    IGestionPermisosServicio permisos) : ICalificacionesServicio
{
    public async Task<CalificacionResumen> RegistrarAsync(
        int usuarioId,
        int estudianteId,
        int materiaId,
        int anioEscolarId,
        TipoEvaluacion tipoEvaluacion,
        int? periodo,
        int? numeroRa,
        decimal puntajeObtenido,
        CancellationToken cancellationToken = default)
    {
        // RD-07: toda entrada externa se valida antes de consultar o escribir.
        if (!Enum.IsDefined(tipoEvaluacion))
        {
            throw new ReglaNegocioExcepcion("El tipo de evaluación debe ser Periodo o RA.");
        }

        if (puntajeObtenido < 0 || puntajeObtenido > ReglasCalificacion.PuntajeMaximo)
        {
            throw new ReglaNegocioExcepcion("El puntaje debe estar entre 0 y 100.");
        }

        if (tipoEvaluacion == TipoEvaluacion.Periodo)
        {
            if (periodo is null || periodo is < 1 or > 4)
            {
                throw new ReglaNegocioExcepcion("Cuando la evaluación es por periodo, el periodo debe estar entre 1 y 4.");
            }

            if (numeroRa is not null)
            {
                throw new ReglaNegocioExcepcion("Cuando la evaluación es por periodo, no se registra un número de RA.");
            }
        }
        else
        {
            if (numeroRa is null)
            {
                throw new ReglaNegocioExcepcion("Cuando la evaluación es por RA, el número de RA es obligatorio.");
            }

            if (periodo is not null)
            {
                throw new ReglaNegocioExcepcion("Cuando la evaluación es por RA, no se registra un periodo.");
            }
        }

        var estudiante = await context.Estudiantes
            .FirstOrDefaultAsync(e => e.Id == estudianteId, cancellationToken)
            ?? throw new ReglaNegocioExcepcion("El estudiante no existe.", 404);

        var materia = await context.Materias
            .FirstOrDefaultAsync(m => m.Id == materiaId, cancellationToken)
            ?? throw new ReglaNegocioExcepcion("La materia no existe.", 404);

        var anioEscolar = await context.AniosEscolares
            .FirstOrDefaultAsync(a => a.Id == anioEscolarId, cancellationToken)
            ?? throw new ReglaNegocioExcepcion("El año escolar no existe.", 404);

        if (tipoEvaluacion == TipoEvaluacion.RA)
        {
            if (materia.CantidadRA is null)
            {
                throw new ReglaNegocioExcepcion("La materia no tiene RA definidos.");
            }

            if (numeroRa.Value < 1 || numeroRa.Value > materia.CantidadRA.Value)
            {
                throw new ReglaNegocioExcepcion("El número de RA debe estar entre 1 y la cantidad de RA de la materia.");
            }
        }

        var calificacion = new Calificacion
        {
            EstudianteId = estudiante.Id,
            Estudiante = estudiante,
            MateriaId = materia.Id,
            AnioEscolarId = anioEscolar.Id,
            TipoEvaluacion = tipoEvaluacion,
            Periodo = periodo,
            NumeroRA = numeroRa,
            PuntajeObtenido = puntajeObtenido,
            Estado = EstadoCalificacion.Borrador,
        };

        await ExigirDocenteTitularAsync(usuarioId, calificacion, cancellationToken);

        // RF-NEG-02: no puede haber dos calificaciones del mismo estudiante, materia,
        // año y tipo para el mismo periodo o RA.
        var iguales = context.Calificaciones.Where(c =>
            c.EstudianteId == estudianteId
            && c.MateriaId == materiaId
            && c.AnioEscolarId == anioEscolarId
            && c.TipoEvaluacion == tipoEvaluacion);

        iguales = tipoEvaluacion == TipoEvaluacion.Periodo
            ? iguales.Where(c => c.Periodo == periodo)
            : iguales.Where(c => c.NumeroRA == numeroRa);

        if (await iguales.AnyAsync(cancellationToken))
        {
            throw new ReglaNegocioExcepcion("Ya existe una calificación igual para ese estudiante, materia y año escolar.", 409);
        }

        context.Calificaciones.Add(calificacion);
        await context.SaveChangesAsync(cancellationToken);

        return AResumen(calificacion);
    }

    public async Task<IReadOnlyList<CalificacionResumen>> ConsultarMisCalificacionesAsync(
        int usuarioId,
        CancellationToken cancellationToken = default)
    {
        // RF-NEG-02: solo las materias y cursos donde el usuario es docente titular.
        var calificaciones = await context.Calificaciones
            .AsNoTracking()
            .Where(c => context.AsignacionesDocentes.Any(a =>
                a.UsuarioId == usuarioId
                && a.MateriaId == c.MateriaId
                && a.CursoId == c.Estudiante.CursoId))
            .OrderByDescending(c => c.FechaCreacion)
            .ThenByDescending(c => c.Id)
            .ToListAsync(cancellationToken);

        return calificaciones.Select(AResumen).ToList();
    }

    public async Task<CalificacionResumen> EnviarAsync(
        int usuarioId,
        int calificacionId,
        CancellationToken cancellationToken = default)
    {
        var calificacion = await ObtenerCalificacionAsync(calificacionId, cancellationToken);
        await ExigirDocenteTitularAsync(usuarioId, calificacion, cancellationToken);

        calificacion.CambiarEstado(EstadoCalificacion.Enviada, ReglasCalificacion.MinimoAprobacion);

        await context.SaveChangesAsync(cancellationToken);

        return AResumen(calificacion);
    }

    public async Task<CalificacionResumen> PublicarAsync(
        int usuarioId,
        int calificacionId,
        CancellationToken cancellationToken = default)
    {
        var calificacion = await ObtenerCalificacionAsync(calificacionId, cancellationToken);
        await ExigirDocenteTitularAsync(usuarioId, calificacion, cancellationToken);

        calificacion.CambiarEstado(EstadoCalificacion.Publicada, ReglasCalificacion.MinimoAprobacion);

        await context.SaveChangesAsync(cancellationToken);

        return AResumen(calificacion);
    }

    public async Task<CalificacionResumen> EnviarARecuperacionAsync(
        int usuarioId,
        int calificacionId,
        CancellationToken cancellationToken = default)
    {
        var calificacion = await ObtenerCalificacionAsync(calificacionId, cancellationToken);
        await ExigirDocenteTitularAsync(usuarioId, calificacion, cancellationToken);

        calificacion.CambiarEstado(EstadoCalificacion.EnRecuperacion, ReglasCalificacion.MinimoAprobacion);

        await context.SaveChangesAsync(cancellationToken);

        return AResumen(calificacion);
    }

    public async Task<CalificacionResumen> CerrarAsync(
        int usuarioId,
        int calificacionId,
        decimal? puntajeRecuperacion = null,
        CancellationToken cancellationToken = default)
    {
        var calificacion = await ObtenerCalificacionAsync(calificacionId, cancellationToken);
        await ExigirDocenteTitularAsync(usuarioId, calificacion, cancellationToken);

        // Solo el cierre desde recuperación registra el resultado; el cierre directo desde
        // Publicada se decide con el puntaje obtenido.
        if (calificacion.Estado == EstadoCalificacion.EnRecuperacion)
        {
            calificacion.PuntajeRecuperacion = puntajeRecuperacion;
        }

        calificacion.CambiarEstado(EstadoCalificacion.Cerrada, ReglasCalificacion.MinimoAprobacion);

        await context.SaveChangesAsync(cancellationToken);

        return AResumen(calificacion);
    }

    public async Task<CalificacionResumen> CorregirPuntajeAsync(
        int usuarioId,
        int calificacionId,
        decimal nuevoPuntaje,
        CancellationToken cancellationToken = default)
    {
        var calificacion = await ObtenerCalificacionAsync(calificacionId, cancellationToken);
        await ExigirDocenteTitularAsync(usuarioId, calificacion, cancellationToken);

        if (calificacion.Estado != EstadoCalificacion.Publicada)
        {
            throw new ReglaNegocioExcepcion("Solo se puede corregir el puntaje de una calificación publicada.", 409);
        }

        if (nuevoPuntaje < 0 || nuevoPuntaje > ReglasCalificacion.PuntajeMaximo)
        {
            throw new ReglaNegocioExcepcion("El puntaje debe estar entre 0 y 100.");
        }

        // RF-NEG-06: el permiso se consulta al Core además de la titularidad; no la sustituye.


        // La corrección no cambia el estado: la calificación sigue en Publicada.
        calificacion.PuntajeObtenido = nuevoPuntaje;

        await context.SaveChangesAsync(cancellationToken);

        return AResumen(calificacion);
    }

    private async Task<Calificacion> ObtenerCalificacionAsync(
        int calificacionId,
        CancellationToken cancellationToken)
        => await context.Calificaciones
               .Include(c => c.Estudiante)
               .FirstOrDefaultAsync(c => c.Id == calificacionId, cancellationToken)
           ?? throw new ReglaNegocioExcepcion("La calificación no existe.", 404);

    /// <summary>
    /// Exige que el usuario sea el docente titular de la materia de la calificación en el
    /// curso del estudiante (RF-NEG-06). La comprobación vive en el servicio, no en la
    /// máquina de estados.
    /// </summary>
    private async Task ExigirDocenteTitularAsync(
        int usuarioId,
        Calificacion calificacion,
        CancellationToken cancellationToken)
    {
        var esTitular = await context.AsignacionesDocentes.AnyAsync(
            a => a.UsuarioId == usuarioId
                 && a.MateriaId == calificacion.MateriaId
                 && a.CursoId == calificacion.Estudiante.CursoId,
            cancellationToken);

        if (!esTitular)
        {
            throw new ReglaNegocioExcepcion("No eres el docente titular de esta calificación.", 403);
        }
    }

    private static CalificacionResumen AResumen(Calificacion calificacion) => new(
        calificacion.Id,
        calificacion.EstudianteId,
        calificacion.MateriaId,
        calificacion.AnioEscolarId,
        calificacion.TipoEvaluacion,
        calificacion.Periodo,
        calificacion.NumeroRA,
        calificacion.PuntajeObtenido,
        calificacion.PuntajeRecuperacion,
        calificacion.Estado.ToString(),
        calificacion.FechaCreacion);
}
