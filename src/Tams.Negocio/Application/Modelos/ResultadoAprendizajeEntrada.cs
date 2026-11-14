namespace Tams.Negocio.Application.Modelos;

/// <summary>
/// Datos de un resultado de aprendizaje que recibe el servicio de materias.
/// </summary>
public record ResultadoAprendizajeEntrada(
    int Numero,
    string? Descripcion,
    int Puntos);
