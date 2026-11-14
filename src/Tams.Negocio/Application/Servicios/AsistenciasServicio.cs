using Core.ControlAcceso.Domain.Excepciones;
using Microsoft.EntityFrameworkCore;
using Tams.Negocio.Application.Modelos;
using Tams.Negocio.Domain.Entidades;
using Tams.Negocio.Domain.Enums;
using Tams.Negocio.Infrastructura.Persistence;

namespace Tams.Negocio.Application.Servicios;

/// <summary>
/// Consulta y guardado de la lista de asistencia de una asignación docente
/// (RF-NEG-02, RD-02, RD-11, RD-12). La fecha de "hoy" se toma del
/// <see cref="TimeProvider"/> inyectado y se compara en UTC.
/// </summary>
public class AsistenciasServicio(
    TamsDbContext context,
    TimeProvider timeProvider) : IAsistenciasServicio
{
    public async Task<ListaAsistenciaResumen> ObtenerListaAsync(
        int usuarioId,
        int asignacionId,
        DateOnly fecha,
        CancellationToken cancellationToken = default)
    {
        var asignacion = await ValidarComunAsync(
            usuarioId,
            asignacionId,
            fecha,
            cancellationToken);

        var estudiantes = await ObtenerEstudiantesAsync(asignacion.CursoId, cancellationToken);

        var asistencias = await context.Asistencias
            .AsNoTracking()
            .Where(a => a.AsignacionDocenteId == asignacionId && a.Fecha == fecha)
            .ToListAsync(cancellationToken);

        var asistenciasPorEstudiante = asistencias.ToDictionary(a => a.EstudianteId);

        var registros = estudiantes
            .Select(estudiante =>
            {
                asistenciasPorEstudiante.TryGetValue(estudiante.Id, out var asistencia);

                return new RegistroAsistenciaResumen(
                    estudiante.Id,
                    estudiante.Nombre,
                    estudiante.Apellido,
                    asistencia?.Estado.ToString(),
                    asistencia?.Observacion);
            })
            .ToList();

        return CrearResumen(
            asignacion,
            fecha,
            guardada: asistencias.Count > 0,
            registros);
    }

    public async Task<ListaAsistenciaResumen> GuardarListaAsync(
        int usuarioId,
        int asignacionId,
        DateOnly fecha,
        IReadOnlyList<RegistroAsistenciaEntrada>? registros,
        CancellationToken cancellationToken = default)
    {
        var asignacion = await ValidarComunAsync(
            usuarioId,
            asignacionId,
            fecha,
            cancellationToken);

        var estudiantes = await ObtenerEstudiantesAsync(asignacion.CursoId, cancellationToken);

        if (estudiantes.Count == 0)
        {
            throw new ReglaNegocioExcepcion("El curso no tiene estudiantes.", 400);
        }

        if (registros is null || registros.Count == 0)
        {
            throw new ReglaNegocioExcepcion(
                "La lista debe incluir a todos los estudiantes del curso.",
                400);
        }

        var idsRegistrados = registros.Select(r => r.EstudianteId).ToList();

        if (idsRegistrados.Count != idsRegistrados.Distinct().Count())
        {
            throw new ReglaNegocioExcepcion(
                "Cada estudiante debe aparecer una sola vez.",
                400);
        }

        var idsDelCurso = estudiantes.Select(e => e.Id).ToHashSet();

        if (idsRegistrados.Any(id => !idsDelCurso.Contains(id)))
        {
            throw new ReglaNegocioExcepcion(
                "La lista solo puede incluir estudiantes del curso.",
                400);
        }

        var faltantes = estudiantes.Count - idsRegistrados.Count;

        if (faltantes > 0)
        {
            throw new ReglaNegocioExcepcion(
                $"La lista debe incluir a todos los estudiantes del curso (faltan {faltantes}).",
                400);
        }

        var registrosValidados = new List<(int EstudianteId, EstadoAsistencia Estado, string? Observacion)>();

        foreach (var registro in registros)
        {
            if (!Enum.TryParse<EstadoAsistencia>(registro.Estado, ignoreCase: true, out var estado)
                || !Enum.IsDefined(estado))
            {
                throw new ReglaNegocioExcepcion(
                    "El estado debe ser Presente, Ausente, Tardanza o Excusa.",
                    400);
            }

            var observacion = registro.Observacion?.Trim();

            if (observacion?.Length > 200)
            {
                throw new ReglaNegocioExcepcion(
                    "La observación admite hasta 200 caracteres.",
                    400);
            }

            registrosValidados.Add((
                registro.EstudianteId,
                estado,
                string.IsNullOrEmpty(observacion) ? null : observacion));
        }

        var asistenciasExistentes = await context.Asistencias
            .Where(a => a.AsignacionDocenteId == asignacionId && a.Fecha == fecha)
            .ToListAsync(cancellationToken);

        var asistenciasPorEstudiante = asistenciasExistentes.ToDictionary(a => a.EstudianteId);

        foreach (var registro in registrosValidados)
        {
            if (asistenciasPorEstudiante.TryGetValue(registro.EstudianteId, out var asistencia))
            {
                asistencia.Estado = registro.Estado;
                asistencia.Observacion = registro.Observacion;
                continue;
            }

            context.Asistencias.Add(new Asistencia
            {
                AsignacionDocenteId = asignacionId,
                EstudianteId = registro.EstudianteId,
                Fecha = fecha,
                Estado = registro.Estado,
                Observacion = registro.Observacion,
            });
        }

        await context.SaveChangesAsync(cancellationToken);

        var registrosPorEstudiante = registrosValidados.ToDictionary(r => r.EstudianteId);

        var resultado = estudiantes
            .Select(estudiante =>
            {
                var registro = registrosPorEstudiante[estudiante.Id];

                return new RegistroAsistenciaResumen(
                    estudiante.Id,
                    estudiante.Nombre,
                    estudiante.Apellido,
                    registro.Estado.ToString(),
                    registro.Observacion);
            })
            .ToList();

        return CrearResumen(asignacion, fecha, guardada: true, resultado);
    }

    /// <summary>
    /// Valida en orden la existencia de la asignación, la titularidad del docente,
    /// el año escolar activo y la fecha (RD-02, RD-11).
    /// </summary>
    private async Task<AsignacionDocente> ValidarComunAsync(
        int usuarioId,
        int asignacionId,
        DateOnly fecha,
        CancellationToken cancellationToken)
    {
        var asignacion = await context.AsignacionesDocentes
            .AsNoTracking()
            .Include(a => a.Materia)
            .Include(a => a.Curso)
            .FirstOrDefaultAsync(a => a.Id == asignacionId, cancellationToken);

        if (asignacion is null)
        {
            throw new ReglaNegocioExcepcion("La asignación no existe.", 404);
        }

        if (asignacion.UsuarioId != usuarioId)
        {
            throw new ReglaNegocioExcepcion("No eres el docente de esta asignación.", 403);
        }

        var anioActivo = await context.AniosEscolares
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.EsActivo, cancellationToken);

        if (anioActivo is null || asignacion.Curso.AnioEscolarId != anioActivo.Id)
        {
            throw new ReglaNegocioExcepcion(
                "Solo se pasa lista en cursos del año escolar activo.",
                400);
        }

        var hoy = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);

        if (fecha > hoy)
        {
            throw new ReglaNegocioExcepcion(
                "No se puede pasar lista de una fecha futura.",
                400);
        }

        if (fecha < anioActivo.FechaInicio || fecha > anioActivo.FechaFin)
        {
            throw new ReglaNegocioExcepcion(
                "La fecha debe estar dentro del año escolar activo.",
                400);
        }

        return asignacion;
    }

    private async Task<List<Estudiante>> ObtenerEstudiantesAsync(
        int cursoId,
        CancellationToken cancellationToken)
        => await context.Estudiantes
            .AsNoTracking()
            .Where(e => e.CursoId == cursoId)
            .OrderBy(e => e.Apellido)
            .ThenBy(e => e.Nombre)
            .ToListAsync(cancellationToken);

    private static ListaAsistenciaResumen CrearResumen(
        AsignacionDocente asignacion,
        DateOnly fecha,
        bool guardada,
        IReadOnlyList<RegistroAsistenciaResumen> registros)
        => new(
            asignacion.Id,
            asignacion.Materia.Nombre,
            asignacion.Curso.Grado,
            asignacion.Curso.Seccion,
            fecha,
            guardada,
            registros);
}
