namespace Tams.Negocio.Application.Servicios;

/// <summary>
/// Fuente de feriados nacionales usada por el módulo de negocio (RD-03, RD-12).
/// La implementación concreta vive en Infrastructura y puede sustituirse por un doble
/// en las pruebas sin acceder a la red.
/// </summary>
public interface IProveedorFeriados
{
    /// <summary>
    /// Devuelve los feriados nacionales de un año calendario. El nombre acompaña a la
    /// fecha para poder mostrarlo en el resumen de días lectivos.
    /// </summary>
    Task<IReadOnlyList<(DateOnly Fecha, string Nombre)>> ObtenerFeriadosNacionalesAsync(
        int anio,
        CancellationToken cancellationToken = default);
}
