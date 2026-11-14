namespace Tams.Negocio.Application.Modelos;

/// <summary>
/// Datos públicos de un resultado de aprendizaje para el listado de materias.
/// </summary>
public record ResultadoAprendizajeResumen(
    int Numero,
    string? Descripcion,
    int Puntos);
