using Core.ControlAcceso.Application.Interfaces;
using Core.ControlAcceso.Domain.Excepciones;
using Microsoft.EntityFrameworkCore;
using Tams.Negocio.Application.Modelos;
using Tams.Negocio.Domain;
using Tams.Negocio.Domain.Entidades;
using Tams.Negocio.Infrastructura.Persistence;

namespace Tams.Negocio.Application.Servicios;

/// <summary>
/// Alta y listado de asignaciones docentes (RF-NEG-02, RD-02, RD-03, RD-12).
/// El docente se consulta al Core por su servicio de lectura; este módulo no lee
/// las tablas del Core (RD-03).
/// </summary>
public class AsignacionesServicio(
    TamsDbContext context,
    IConsultaUsuariosServicio consultaUsuarios) : IAsignacionesServicio
{
    public async Task<AsignacionAdministracionResumen> CrearAsync(
        int usuarioId,
        int materiaId,
        int cursoId,
        int horasSemanales,
        CancellationToken cancellationToken = default)
    {
        if (horasSemanales < ReglasAsignacion.HorasSemanalesMinimas
            || horasSemanales > ReglasAsignacion.HorasSemanalesMaximas)
        {
            throw new ReglaNegocioExcepcion(
                $"Las horas semanales deben estar entre {ReglasAsignacion.HorasSemanalesMinimas} y {ReglasAsignacion.HorasSemanalesMaximas}.",
                400);
        }

        var materia = await context.Materias
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == materiaId, cancellationToken);

        if (materia is null)
        {
            throw new ReglaNegocioExcepcion("La materia no existe.", 404);
        }

        var curso = await context.Cursos
            .AsNoTracking()
            .Include(c => c.AnioEscolar)
            .FirstOrDefaultAsync(c => c.Id == cursoId, cancellationToken);

        if (curso is null)
        {
            throw new ReglaNegocioExcepcion("El curso no existe.", 404);
        }

        var docente = await consultaUsuarios.ObtenerAsync(usuarioId, cancellationToken);

        if (docente is null)
        {
            throw new ReglaNegocioExcepcion("El docente no existe.", 404);
        }

        if (!string.Equals(docente.Rol, "Estandar", StringComparison.OrdinalIgnoreCase)
            || !docente.Activo)
        {
            throw new ReglaNegocioExcepcion(
                "El usuario debe ser un docente (rol Estándar) con la cuenta activa.",
                400);
        }

        if (materia.EsTecnica && !curso.EsTecnico)
        {
            throw new ReglaNegocioExcepcion(
                "Una materia técnica solo puede asignarse a un curso técnico.",
                400);
        }

        if (materia.TipoTecnico is not null
            && curso.TipoTecnico is not null
            && materia.TipoTecnico != curso.TipoTecnico)
        {
            throw new ReglaNegocioExcepcion(
                "El tipo técnico de la materia y del curso deben coincidir.",
                400);
        }

        var yaAsignada = await context.AsignacionesDocentes
            .AsNoTracking()
            .AnyAsync(
                a => a.MateriaId == materiaId && a.CursoId == cursoId,
                cancellationToken);

        if (yaAsignada)
        {
            throw new ReglaNegocioExcepcion(
                "Esa materia ya tiene un docente asignado en este curso.",
                409);
        }

        var asignacion = new AsignacionDocente
        {
            UsuarioId = usuarioId,
            MateriaId = materiaId,
            CursoId = cursoId,
            HorasSemanales = horasSemanales,
        };

        context.AsignacionesDocentes.Add(asignacion);
        await context.SaveChangesAsync(cancellationToken);

        return new AsignacionAdministracionResumen(
            asignacion.Id,
            asignacion.UsuarioId,
            docente.Nombre,
            docente.Correo,
            materia.Id,
            materia.Nombre,
            curso.Id,
            curso.Grado,
            curso.Seccion,
            curso.AnioEscolarId,
            curso.AnioEscolar.Nombre,
            asignacion.HorasSemanales);
    }

    public async Task<IReadOnlyList<AsignacionAdministracionResumen>> ListarAsync(
        int? cursoId,
        int? usuarioId,
        CancellationToken cancellationToken = default)
    {
        var consulta = context.AsignacionesDocentes.AsNoTracking();

        if (cursoId.HasValue)
        {
            consulta = consulta.Where(a => a.CursoId == cursoId.Value);
        }

        if (usuarioId.HasValue)
        {
            consulta = consulta.Where(a => a.UsuarioId == usuarioId.Value);
        }

        var filas = await consulta
            .OrderBy(a => a.Curso.AnioEscolar.Nombre)
            .ThenBy(a => a.Curso.Grado)
            .ThenBy(a => a.Curso.Seccion)
            .ThenBy(a => a.Materia.Nombre)
            .Select(a => new
            {
                a.Id,
                a.UsuarioId,
                a.MateriaId,
                MateriaNombre = a.Materia.Nombre,
                a.CursoId,
                a.Curso.Grado,
                a.Curso.Seccion,
                a.Curso.AnioEscolarId,
                AnioEscolarNombre = a.Curso.AnioEscolar.Nombre,
                a.HorasSemanales,
            })
            .ToListAsync(cancellationToken);

        var usuarios = await consultaUsuarios.ObtenerVariosAsync(
            filas.Select(f => f.UsuarioId).Distinct().ToList(),
            cancellationToken);

        var docentesPorId = usuarios.ToDictionary(u => u.Id);

        return filas
            .Select(fila =>
            {
                docentesPorId.TryGetValue(fila.UsuarioId, out var docente);

                return new AsignacionAdministracionResumen(
                    fila.Id,
                    fila.UsuarioId,
                    docente?.Nombre,
                    docente?.Correo,
                    fila.MateriaId,
                    fila.MateriaNombre,
                    fila.CursoId,
                    fila.Grado,
                    fila.Seccion,
                    fila.AnioEscolarId,
                    fila.AnioEscolarNombre,
                    fila.HorasSemanales);
            })
            .ToList();
    }
}
