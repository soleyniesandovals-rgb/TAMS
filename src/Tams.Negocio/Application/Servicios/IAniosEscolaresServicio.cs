using Tams.Negocio.Application.Modelos;

namespace Tams.Negocio.Application.Servicios;

/// <summary>
/// Altas y activación de años escolares (RF-NEG-02, RD-02, RD-06).
/// La lógica vive aquí, no en el controlador.
/// </summary>
public interface IAniosEscolaresServicio
{
    /// <summary>Crea un año escolar inactivo y devuelve su resumen.</summary>
    Task<AnioEscolarResumen> CrearAsync(
        string nombre,
        DateOnly fechaInicio,
        DateOnly fechaFin,
        CancellationToken cancellationToken = default);

    /// <summary>Activa un año escolar y desactiva el que estuviera activo.</summary>
    Task<AnioEscolarResumen> ActivarAsync(
        int id,
        CancellationToken cancellationToken = default);
}
