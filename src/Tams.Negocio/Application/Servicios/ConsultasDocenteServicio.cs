using Core.ControlAcceso.Domain.Excepciones;
using Microsoft.EntityFrameworkCore;
using Tams.Negocio.Application.Modelos;
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
}
