using Core.ControlAcceso.Domain.Enums;
using Core.Reportes.Application;
using Microsoft.EntityFrameworkCore;
using Tams.Negocio.Application.Modelos;
using Tams.Negocio.Domain.Enums;
using Tams.Negocio.Infrastructura.Persistence;

namespace Tams.Negocio.Application.Servicios;

/// <summary>
/// Reporte del negocio: «¿qué porcentaje de asistencia tiene cada estudiante en
/// cada materia?» (RF-REP-02, RF-REP-03, RF-REP-04, RF-REP-05, RD-02).
/// Los conteos los hace la base con <c>GroupBy</c> y conteos condicionales sobre
/// <c>IQueryable</c>; el porcentaje se calcula en C# sobre las filas ya agrupadas.
/// RD-03: reutiliza el rango del Core, sin que el Core dependa del negocio.
/// </summary>
public class ReporteAsistenciaServicio(TamsDbContext context) : IReporteAsistenciaServicio
{
    public async Task<IReadOnlyList<AsistenciaEstudianteResumen>> ObtenerPorEstudianteYMateriaAsync(
        int usuarioId,
        RolUsuario rol,
        string? desde,
        string? hasta,
        CancellationToken cancellationToken = default)
    {
        // RF-REP-03/RD-07: la misma validación de rango que en el reporte del Core.
        var rango = RangoFechasReporte.Crear(desde, hasta);

        // RF-CA-05: el controlador no decide quién ve qué; el servicio filtra por
        // rol. Administrador ve todas las asignaciones; Estándar (docente) solo
        // las asignaciones cuyo UsuarioId es el suyo.
        var consulta = context.Asistencias
            .AsNoTracking()
            .Where(a => a.Fecha >= rango.Desde && a.Fecha <= rango.Hasta);

        if (rol != RolUsuario.Administrador)
        {
            consulta = consulta.Where(a => a.AsignacionDocente.UsuarioId == usuarioId);
        }

        var agrupados = await consulta
            .GroupBy(a => new
            {
                a.AsignacionDocenteId,
                a.EstudianteId,
                MateriaNombre = a.AsignacionDocente.Materia.Nombre,
                Grado = a.AsignacionDocente.Curso.Grado,
                Seccion = a.AsignacionDocente.Curso.Seccion,
                Apellido = a.Estudiante.Apellido,
                Nombre = a.Estudiante.Nombre,
            })
            .Select(g => new
            {
                g.Key.AsignacionDocenteId,
                g.Key.EstudianteId,
                g.Key.MateriaNombre,
                g.Key.Grado,
                g.Key.Seccion,
                g.Key.Apellido,
                g.Key.Nombre,
                Presentes = g.Count(a => a.Estado == EstadoAsistencia.Presente),
                Tardanzas = g.Count(a => a.Estado == EstadoAsistencia.Tardanza),
                Ausencias = g.Count(a => a.Estado == EstadoAsistencia.Ausente),
                Excusas = g.Count(a => a.Estado == EstadoAsistencia.Excusa),
                Total = g.Count(),
            })
            .ToListAsync(cancellationToken);

        // C-21: el porcentaje se calcula en C# sobre filas ya agregadas por la base.
        return agrupados
            .OrderBy(f => f.MateriaNombre, StringComparer.Ordinal)
            .ThenBy(f => f.Grado, StringComparer.Ordinal)
            .ThenBy(f => f.Seccion, StringComparer.Ordinal)
            .ThenBy(f => f.Apellido, StringComparer.Ordinal)
            .ThenBy(f => f.Nombre, StringComparer.Ordinal)
            .Select(f => new AsistenciaEstudianteResumen(
                f.MateriaNombre,
                f.Grado,
                f.Seccion,
                f.Nombre,
                f.Apellido,
                f.Presentes,
                f.Tardanzas,
                f.Ausencias,
                f.Excusas,
                f.Total,
                CalcularPorcentaje(f.Presentes, f.Tardanzas, f.Total)))
            .ToList();
    }

    private static decimal CalcularPorcentaje(int presentes, int tardanzas, int total)
        => total == 0
            ? 0m
            : Math.Round(100m * (presentes + tardanzas) / total, 2, MidpointRounding.AwayFromZero);
}
