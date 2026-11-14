using Core.ControlAcceso.Domain.Excepciones;
using Microsoft.EntityFrameworkCore;
using Tams.Negocio.Application.Modelos;
using Tams.Negocio.Domain.Entidades;
using Tams.Negocio.Domain.Enums;
using Tams.Negocio.Infrastructura.Persistence;

namespace Tams.Negocio.Application.Servicios;

/// <summary>
/// Alta y listado de materias con sus resultados de aprendizaje
/// (RF-NEG-01, RF-NEG-02, RD-02, RD-06, RD-12).
/// Los RA aportan el peso de cada resultado en una futura nota final ponderada;
/// la calificación de cada RA sigue siendo de 0 a 100 y no cambia aquí.
/// </summary>
public class MateriasServicio(TamsDbContext context) : IMateriasServicio
{
    private const int LongitudMaximaNombre = 150;
    private const int LongitudMaximaDescripcion = 200;
    private const int PuntosTotales = 100;

    public async Task<MateriaResumen> CrearAsync(
        string nombre,
        bool esTecnica,
        string? tipoTecnico,
        IReadOnlyList<ResultadoAprendizajeEntrada>? resultadosAprendizaje,
        CancellationToken cancellationToken = default)
    {
        var nombreNormalizado = nombre?.Trim() ?? string.Empty;

        if (nombreNormalizado.Length == 0 || nombreNormalizado.Length > LongitudMaximaNombre)
        {
            throw new ReglaNegocioExcepcion(
                "El nombre de la materia es obligatorio y admite hasta 150 caracteres.",
                400);
        }

        var resultados = resultadosAprendizaje ?? [];
        TipoTecnico? tipoTecnicoNormalizado = null;

        if (!esTecnica)
        {
            if (!string.IsNullOrWhiteSpace(tipoTecnico) || resultados.Count > 0)
            {
                throw new ReglaNegocioExcepcion(
                    "Una materia no técnica no lleva tipo técnico ni resultados de aprendizaje.",
                    400);
            }
        }
        else
        {
            if (!Enum.TryParse<TipoTecnico>(tipoTecnico?.Trim(), ignoreCase: true, out var tipo)
                || !Enum.IsDefined(tipo))
            {
                throw new ReglaNegocioExcepcion(
                    "El tipo técnico es obligatorio en una materia técnica y debe ser ComercioYMercadeo o DesarrolloAdministracionAplicacionesInformaticas.",
                    400);
            }

            tipoTecnicoNormalizado = tipo;

            if (resultados.Count == 0)
            {
                throw new ReglaNegocioExcepcion(
                    "Una materia técnica necesita al menos un resultado de aprendizaje.",
                    400);
            }

            // Los números ordenados deben ser exactamente 1, 2, …, n.
            var numeros = resultados
                .Select(r => r.Numero)
                .OrderBy(numero => numero)
                .ToList();

            if (!numeros.Select((numero, indice) => numero == indice + 1).All(valido => valido))
            {
                throw new ReglaNegocioExcepcion(
                    "Los resultados de aprendizaje deben numerarse 1, 2, 3… sin repetir ni saltar números.",
                    400);
            }

            if (resultados.Any(r => r.Puntos is < 1 or > 100))
            {
                throw new ReglaNegocioExcepcion(
                    "Cada resultado de aprendizaje vale entre 1 y 100 puntos.",
                    400);
            }

            if (resultados.Any(r => (r.Descripcion?.Trim().Length ?? 0) > LongitudMaximaDescripcion))
            {
                throw new ReglaNegocioExcepcion(
                    "La descripción de un resultado de aprendizaje admite hasta 200 caracteres.",
                    400);
            }

            var suma = resultados.Sum(r => r.Puntos);

            if (suma != PuntosTotales)
            {
                throw new ReglaNegocioExcepcion(
                    $"Los puntos de los resultados de aprendizaje deben sumar 100 (suman {suma}).",
                    400);
            }
        }

        // Se compara en minúsculas para que también funcione con el proveedor InMemory.
        var nombreEnMinusculas = nombreNormalizado.ToLower();
        var yaExiste = await context.Materias
            .AsNoTracking()
            .AnyAsync(m => m.Nombre.ToLower() == nombreEnMinusculas, cancellationToken);

        if (yaExiste)
        {
            throw new ReglaNegocioExcepcion("Ya existe una materia con ese nombre.", 409);
        }

        var materia = new Materia
        {
            Nombre = nombreNormalizado,
            EsTecnica = esTecnica,
            TipoTecnico = tipoTecnicoNormalizado,
            CantidadRA = esTecnica ? resultados.Count : null,
        };

        foreach (var resultado in resultados.OrderBy(r => r.Numero))
        {
            materia.ResultadosAprendizaje.Add(new ResultadoAprendizaje
            {
                Numero = resultado.Numero,
                Descripcion = string.IsNullOrWhiteSpace(resultado.Descripcion)
                    ? null
                    : resultado.Descripcion.Trim(),
                Puntos = resultado.Puntos,
            });
        }

        context.Materias.Add(materia);
        await context.SaveChangesAsync(cancellationToken);

        return Resumir(materia);
    }

    public async Task<IReadOnlyList<MateriaResumen>> ListarAsync(
        CancellationToken cancellationToken = default)
    {
        var materias = await context.Materias
            .AsNoTracking()
            .Include(m => m.ResultadosAprendizaje)
            .OrderBy(m => m.Nombre)
            .ToListAsync(cancellationToken);

        return materias.Select(Resumir).ToList();
    }

    private static MateriaResumen Resumir(Materia materia) => new(
        materia.Id,
        materia.Nombre,
        materia.EsTecnica,
        materia.TipoTecnico,
        materia.CantidadRA,
        materia.ResultadosAprendizaje
            .OrderBy(r => r.Numero)
            .Select(r => new ResultadoAprendizajeResumen(r.Numero, r.Descripcion, r.Puntos))
            .ToList());
}
