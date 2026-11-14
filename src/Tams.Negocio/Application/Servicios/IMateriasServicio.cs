using Tams.Negocio.Application.Modelos;

namespace Tams.Negocio.Application.Servicios;

/// <summary>
/// Alta y listado de materias con sus resultados de aprendizaje (RF-NEG-01, RF-NEG-02,
/// RD-02, RD-06, RD-12). La lógica vive aquí, no en el controlador.
/// </summary>
public interface IMateriasServicio
{
    /// <summary>Crea una materia y, si es técnica, sus resultados de aprendizaje.</summary>
    Task<MateriaResumen> CrearAsync(
        string nombre,
        bool esTecnica,
        string? tipoTecnico,
        IReadOnlyList<ResultadoAprendizajeEntrada>? resultadosAprendizaje,
        CancellationToken cancellationToken = default);

    /// <summary>Lista todas las materias con sus resultados de aprendizaje.</summary>
    Task<IReadOnlyList<MateriaResumen>> ListarAsync(
        CancellationToken cancellationToken = default);
}
