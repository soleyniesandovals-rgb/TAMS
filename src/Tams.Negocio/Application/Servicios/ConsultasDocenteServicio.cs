using Core.ControlAcceso.Domain.Excepciones;
using Microsoft.EntityFrameworkCore;
using Tams.Negocio.Application.Modelos;
using Tams.Negocio.Domain;
using Tams.Negocio.Domain.Enums;
using Tams.Negocio.Infrastructura.Persistence;

namespace Tams.Negocio.Application.Servicios;

/// <summary>
/// Consultas de solo lectura del docente (RF-NEG-02, RD-02, RD-12).
/// La lógica vive aquí y no en el controlador.
/// </summary>
public class ConsultasDocenteServicio(TamsDbContext context) : IConsultasDocenteServicio
{
    public async Task<IReadOnlyList<AsignacionDocenteResumen>> ListarMisAsignacionesAsync(
        int usuarioId,
        CancellationToken cancellationToken = default)
    {
        var asignaciones = await context.AsignacionesDocentes
            .AsNoTracking()
            .Where(a => a.UsuarioId == usuarioId)
            .OrderBy(a => a.Curso.AnioEscolar.FechaInicio)
            .ThenBy(a => a.Curso.Grado)
            .ThenBy(a => a.Curso.Seccion)
            .ThenBy(a => a.Materia.Nombre)
            .Select(a => new AsignacionDocenteResumen(
                a.Id,
                a.MateriaId,
                a.Materia.Nombre,
                a.Materia.EsTecnica,
                a.Materia.CantidadRA,
                a.CursoId,
                a.Curso.Grado,
                a.Curso.Seccion,
                a.Curso.AnioEscolarId,
                a.Curso.AnioEscolar.Nombre,
                a.HorasSemanales))
            .ToListAsync(cancellationToken);

        return asignaciones;
    }

    public async Task<IReadOnlyList<EstudianteResumen>> ListarEstudiantesDeCursoAsync(
        int usuarioId,
        int cursoId,
        CancellationToken cancellationToken = default)
    {
        var cursoExiste = await context.Cursos
            .AsNoTracking()
            .AnyAsync(c => c.Id == cursoId, cancellationToken);

        if (!cursoExiste)
        {
            throw new ReglaNegocioExcepcion("El curso no existe.", 404);
        }

        var tieneAsignacion = await context.AsignacionesDocentes
            .AsNoTracking()
            .AnyAsync(a => a.UsuarioId == usuarioId && a.CursoId == cursoId, cancellationToken);

        if (!tieneAsignacion)
        {
            throw new ReglaNegocioExcepcion("No tienes asignaciones en este curso.", 403);
        }

        var estudiantes = await context.Estudiantes
            .AsNoTracking()
            .Where(e => e.CursoId == cursoId)
            .OrderBy(e => e.Apellido)
            .ThenBy(e => e.Nombre)
            .Select(e => new EstudianteResumen(e.Id, e.Nombre, e.Apellido, e.CursoId))
            .ToListAsync(cancellationToken);

        return estudiantes;
    }

    public async Task<AnioEscolarResumen> ObtenerAnioEscolarActivoAsync(
        CancellationToken cancellationToken = default)
    {
        var anioActivo = await context.AniosEscolares
            .AsNoTracking()
            .Where(a => a.EsActivo)
            .Select(a => new AnioEscolarResumen(a.Id, a.Nombre, a.FechaInicio, a.FechaFin, a.EsActivo))
            .FirstOrDefaultAsync(cancellationToken);

        if (anioActivo is null)
        {
            throw new ReglaNegocioExcepcion("No hay un año escolar activo.", 404);
        }

        return anioActivo;
    }

    public async Task<TablaCalificacionesResumen> ObtenerTablaCalificacionesAsync(
        int usuarioId,
        int asignacionId,
        CancellationToken cancellationToken = default)
    {
        var asignacion = await context.AsignacionesDocentes
            .AsNoTracking()
            .Where(a => a.Id == asignacionId)
            .Select(a => new
            {
                a.UsuarioId,
                a.MateriaId,
                MateriaNombre = a.Materia.Nombre,
                a.Materia.EsTecnica,
                a.CursoId,
                a.Curso.Grado,
                a.Curso.Seccion,
                a.Curso.AnioEscolarId,
                AnioEscolarNombre = a.Curso.AnioEscolar.Nombre,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (asignacion is null)
        {
            throw new ReglaNegocioExcepcion("La asignación no existe.", 404);
        }

        if (asignacion.UsuarioId != usuarioId)
        {
            throw new ReglaNegocioExcepcion("No eres el docente de esta asignación.", 403);
        }

        var tipoEvaluacion = asignacion.EsTecnica
            ? TipoEvaluacion.RA
            : TipoEvaluacion.Periodo;

        var resultadosAprendizaje = asignacion.EsTecnica
            ? await context.ResultadosAprendizaje
                .AsNoTracking()
                .Where(ra => ra.MateriaId == asignacion.MateriaId)
                .OrderBy(ra => ra.Numero)
                .Select(ra => new { ra.Numero, ra.Puntos })
                .ToListAsync(cancellationToken)
            : [];

        var columnas = asignacion.EsTecnica
            ? resultadosAprendizaje
                .Select(ra => new ColumnaCalificacionResumen(
                    TipoEvaluacion.RA,
                    ra.Numero,
                    $"RA{ra.Numero}",
                    ra.Puntos))
                .ToList()
            : Enumerable.Range(1, 4)
                .Select(numero => new ColumnaCalificacionResumen(
                    TipoEvaluacion.Periodo,
                    numero,
                    $"P{numero}",
                    null))
                .ToList();

        var puntosPorNumero = columnas.ToDictionary(c => c.Numero, c => c.Puntos ?? 0);

        var estudiantes = await context.Estudiantes
            .AsNoTracking()
            .Where(e => e.CursoId == asignacion.CursoId)
            .OrderBy(e => e.Apellido)
            .ThenBy(e => e.Nombre)
            .Select(e => new { e.Id, e.Nombre, e.Apellido })
            .ToListAsync(cancellationToken);

        var estudianteIds = estudiantes.Select(e => e.Id).ToList();

        // Cuentan todas las calificaciones registradas, en cualquier estado.
        var calificaciones = await context.Calificaciones
            .AsNoTracking()
            .Where(c => estudianteIds.Contains(c.EstudianteId)
                && c.MateriaId == asignacion.MateriaId
                && c.AnioEscolarId == asignacion.AnioEscolarId
                && c.TipoEvaluacion == tipoEvaluacion)
            .Select(c => new
            {
                c.Id,
                c.EstudianteId,
                Numero = c.NumeroRA ?? c.Periodo ?? 0,
                c.PuntajeObtenido,
                c.PuntajeRecuperacion,
                c.Estado,
            })
            .ToListAsync(cancellationToken);

        var notasPorEstudiante = calificaciones
            .GroupBy(c => c.EstudianteId)
            .ToDictionary(
                grupo => grupo.Key,
                grupo => grupo
                    .OrderBy(c => c.Numero)
                    .Select(c => new NotaCalificacionResumen(
                        c.Numero,
                        c.Id,
                        c.PuntajeObtenido,
                        c.PuntajeRecuperacion,
                        ReglasAcumulado.PuntajeEfectivo(c.PuntajeObtenido, c.PuntajeRecuperacion),
                        c.Estado.ToString()))
                    .Where(nota => puntosPorNumero.ContainsKey(nota.Numero))
                    .ToList());

        var filas = estudiantes
            .Select(e =>
            {
                var notas = notasPorEstudiante.TryGetValue(e.Id, out var existentes)
                    ? existentes
                    : [];

                var acumulado = asignacion.EsTecnica
                    ? ReglasAcumulado.AcumuladoRa(notas.Select(nota => (
                        nota.PuntajeEfectivo,
                        puntosPorNumero[nota.Numero])))
                    : ReglasAcumulado.AcumuladoPeriodos(notas.Select(nota => nota.PuntajeEfectivo));

                return new EstudianteCalificacionesResumen(
                    e.Id,
                    e.Nombre,
                    e.Apellido,
                    notas,
                    acumulado);
            })
            .ToList();

        return new TablaCalificacionesResumen(
            asignacionId,
            asignacion.MateriaId,
            asignacion.MateriaNombre,
            asignacion.EsTecnica,
            asignacion.Grado,
            asignacion.Seccion,
            asignacion.AnioEscolarId,
            asignacion.AnioEscolarNombre,
            columnas,
            filas);
    }
}
