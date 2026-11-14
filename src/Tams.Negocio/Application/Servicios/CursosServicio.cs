using Core.ControlAcceso.Domain.Excepciones;
using Microsoft.EntityFrameworkCore;
using Tams.Negocio.Application.Modelos;
using Tams.Negocio.Domain.Entidades;
using Tams.Negocio.Domain.Enums;
using Tams.Negocio.Infrastructura.Persistence;

namespace Tams.Negocio.Application.Servicios;

/// <summary>
/// Alta y listado de cursos y de los estudiantes de cada curso
/// (RF-NEG-01, RF-NEG-02, RD-02, RD-06, RD-07, RD-08, RD-12).
/// La cédula es un dato personal de un menor: solo se devuelve al Administrador y
/// nunca se escribe en logs ni en mensajes de error.
/// </summary>
public class CursosServicio(TamsDbContext context) : ICursosServicio
{
    private const int LongitudMaximaGrado = 30;
    private const int LongitudMaximaSeccion = 10;
    private const int LongitudMaximaNombre = 100;
    private const int LongitudCedula = 11;

    public async Task<CursoResumen> CrearCursoAsync(
        string grado,
        string seccion,
        bool esTecnico,
        string? tipoTecnico,
        int anioEscolarId,
        CancellationToken cancellationToken = default)
    {
        var gradoNormalizado = grado?.Trim() ?? string.Empty;

        if (gradoNormalizado.Length == 0 || gradoNormalizado.Length > LongitudMaximaGrado)
        {
            throw new ReglaNegocioExcepcion(
                "El grado es obligatorio y admite hasta 30 caracteres.",
                400);
        }

        var seccionNormalizada = seccion?.Trim() ?? string.Empty;

        if (seccionNormalizada.Length == 0 || seccionNormalizada.Length > LongitudMaximaSeccion)
        {
            throw new ReglaNegocioExcepcion(
                "La sección es obligatoria y admite hasta 10 caracteres.",
                400);
        }

        TipoTecnico? tipoTecnicoNormalizado = null;

        if (!esTecnico)
        {
            if (!string.IsNullOrWhiteSpace(tipoTecnico))
            {
                throw new ReglaNegocioExcepcion(
                    "Un curso no técnico no lleva tipo técnico.",
                    400);
            }
        }
        else if (!string.IsNullOrWhiteSpace(tipoTecnico))
        {
            // El tipo técnico es opcional, pero si viene debe ser un valor del enum.
            if (!Enum.TryParse<TipoTecnico>(tipoTecnico.Trim(), ignoreCase: true, out var tipo)
                || !Enum.IsDefined(tipo))
            {
                throw new ReglaNegocioExcepcion(
                    "El tipo técnico debe ser ComercioYMercadeo o DesarrolloAdministracionAplicacionesInformaticas.",
                    400);
            }

            tipoTecnicoNormalizado = tipo;
        }

        var anioEscolar = await context.AniosEscolares
            .FirstOrDefaultAsync(a => a.Id == anioEscolarId, cancellationToken)
            ?? throw new ReglaNegocioExcepcion("El año escolar no existe.", 404);

        // Se compara en minúsculas para que también funcione con el proveedor InMemory.
        var gradoEnMinusculas = gradoNormalizado.ToLower();
        var seccionEnMinusculas = seccionNormalizada.ToLower();
        var yaExiste = await context.Cursos
            .AsNoTracking()
            .AnyAsync(
                c => c.AnioEscolarId == anioEscolarId
                    && c.Grado.ToLower() == gradoEnMinusculas
                    && c.Seccion.ToLower() == seccionEnMinusculas,
                cancellationToken);

        if (yaExiste)
        {
            throw new ReglaNegocioExcepcion(
                "Ya existe un curso con ese grado y sección en ese año escolar.",
                409);
        }

        var curso = new Curso
        {
            Grado = gradoNormalizado,
            Seccion = seccionNormalizada,
            EsTecnico = esTecnico,
            TipoTecnico = tipoTecnicoNormalizado,
            AnioEscolarId = anioEscolarId,
            AnioEscolar = anioEscolar,
        };

        context.Cursos.Add(curso);
        await context.SaveChangesAsync(cancellationToken);

        return Resumir(curso);
    }

    public async Task<IReadOnlyList<CursoResumen>> ListarCursosAsync(
        int? anioEscolarId,
        CancellationToken cancellationToken = default)
    {
        var consulta = context.Cursos.AsNoTracking();

        if (anioEscolarId.HasValue)
        {
            consulta = consulta.Where(c => c.AnioEscolarId == anioEscolarId.Value);
        }

        return await consulta
            .OrderBy(c => c.AnioEscolar.Nombre)
            .ThenBy(c => c.Grado)
            .ThenBy(c => c.Seccion)
            .Select(c => new CursoResumen(
                c.Id,
                c.Grado,
                c.Seccion,
                c.EsTecnico,
                c.TipoTecnico,
                c.AnioEscolarId,
                c.AnioEscolar.Nombre))
            .ToListAsync(cancellationToken);
    }

    public async Task<EstudianteAdministracionResumen> CrearEstudianteAsync(
        int cursoId,
        string nombre,
        string apellido,
        string? cedula,
        CancellationToken cancellationToken = default)
    {
        var nombreNormalizado = nombre?.Trim() ?? string.Empty;

        if (nombreNormalizado.Length == 0 || nombreNormalizado.Length > LongitudMaximaNombre)
        {
            throw new ReglaNegocioExcepcion(
                "El nombre y el apellido son obligatorios y admiten hasta 100 caracteres.",
                400);
        }

        var apellidoNormalizado = apellido?.Trim() ?? string.Empty;

        if (apellidoNormalizado.Length == 0 || apellidoNormalizado.Length > LongitudMaximaNombre)
        {
            throw new ReglaNegocioExcepcion(
                "El nombre y el apellido son obligatorios y admiten hasta 100 caracteres.",
                400);
        }

        var cedulaNormalizada = NormalizarCedula(cedula);

        if (cedulaNormalizada.Length != LongitudCedula
            || !cedulaNormalizada.All(caracter => caracter is >= '0' and <= '9'))
        {
            throw new ReglaNegocioExcepcion(
                "La cédula es obligatoria y debe tener 11 dígitos (con o sin guiones).",
                400);
        }

        var cursoExiste = await context.Cursos
            .AsNoTracking()
            .AnyAsync(c => c.Id == cursoId, cancellationToken);

        if (!cursoExiste)
        {
            throw new ReglaNegocioExcepcion("El curso no existe.", 404);
        }

        var yaExiste = await context.Estudiantes
            .AsNoTracking()
            .AnyAsync(e => e.Cedula == cedulaNormalizada, cancellationToken);

        if (yaExiste)
        {
            throw new ReglaNegocioExcepcion("Ya existe un estudiante con esa cédula.", 409);
        }

        var estudiante = new Estudiante
        {
            Nombre = nombreNormalizado,
            Apellido = apellidoNormalizado,
            Cedula = cedulaNormalizada,
            CursoId = cursoId,
        };

        context.Estudiantes.Add(estudiante);
        await context.SaveChangesAsync(cancellationToken);

        return Resumir(estudiante);
    }

    public async Task<IReadOnlyList<EstudianteAdministracionResumen>> ListarEstudiantesAsync(
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

        return await context.Estudiantes
            .AsNoTracking()
            .Where(e => e.CursoId == cursoId)
            .OrderBy(e => e.Apellido)
            .ThenBy(e => e.Nombre)
            .Select(e => new EstudianteAdministracionResumen(
                e.Id,
                e.Nombre,
                e.Apellido,
                e.Cedula,
                e.CursoId))
            .ToListAsync(cancellationToken);
    }

    /// <summary>Quita guiones y espacios; la cédula se guarda solo con sus 11 dígitos.</summary>
    private static string NormalizarCedula(string? cedula)
    {
        var sinGuiones = (cedula ?? string.Empty).Replace("-", string.Empty);

        return new string(sinGuiones.Where(caracter => !char.IsWhiteSpace(caracter)).ToArray());
    }

    private static CursoResumen Resumir(Curso curso) => new(
        curso.Id,
        curso.Grado,
        curso.Seccion,
        curso.EsTecnico,
        curso.TipoTecnico,
        curso.AnioEscolarId,
        curso.AnioEscolar.Nombre);

    private static EstudianteAdministracionResumen Resumir(Estudiante estudiante) => new(
        estudiante.Id,
        estudiante.Nombre,
        estudiante.Apellido,
        estudiante.Cedula,
        estudiante.CursoId);
}
