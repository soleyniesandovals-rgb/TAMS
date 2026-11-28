namespace Tams.Negocio.Domain;

/// <summary>
/// Reglas puras de los eventos del calendario (RF-NEG, RD-02). Los límites y la
/// normalización viven aquí para que el servicio y sus pruebas compartan una sola
/// definición, sin depender de EF ni de la API (RD-04).
/// </summary>
public static class ReglasEventoCalendario
{
    /// <summary>Longitud máxima de la descripción de un evento, incluida.</summary>
    public const int LongitudMaximaDescripcion = 200;

    /// <summary>
    /// Recorta los espacios sobrantes de la descripción. Devuelve una cadena vacía
    /// cuando la entrada es nula o solo contiene espacios.
    /// </summary>
    public static string NormalizarDescripcion(string? descripcion)
        => descripcion?.Trim() ?? string.Empty;

    /// <summary>
    /// La fecha del evento debe caer dentro del año escolar, ambos extremos incluidos.
    /// Se admiten fechas pasadas y varios eventos el mismo día.
    /// </summary>
    public static bool EsFechaValida(DateOnly fecha, DateOnly fechaInicio, DateOnly fechaFin)
        => fecha >= fechaInicio && fecha <= fechaFin;

    /// <summary>La descripción normalizada es obligatoria y no supera 200 caracteres.</summary>
    public static bool EsDescripcionValida(string descripcionNormalizada)
        => descripcionNormalizada.Length > 0
           && descripcionNormalizada.Length <= LongitudMaximaDescripcion;
}
