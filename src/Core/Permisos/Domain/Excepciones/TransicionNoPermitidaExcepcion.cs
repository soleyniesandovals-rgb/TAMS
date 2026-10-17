using Core.Permisos.Domain.Enums;

namespace Core.Permisos.Domain.Excepciones;

/// <summary>
/// Transición de estado que la máquina de solicitudes de permiso no permite (RF-GP-06).
/// Es un error de negocio con mensaje controlado para el usuario final (RD-07, RD-08);
/// la capa que la reciba la traduce a respuesta HTTP (por ejemplo, 409 Conflict).
///
/// RD-03: copia propia del Core. No se reutiliza la de <c>Tams.Negocio</c> porque el
/// Core no puede depender del módulo de negocio.
/// </summary>
public class TransicionNoPermitidaExcepcion : Exception
{
    public TransicionNoPermitidaExcepcion(EstadoSolicitudPermiso origen, EstadoSolicitudPermiso destino, string? motivo = null)
        : base($"No se permite pasar de {origen} a {destino}. Motivo: {motivo ?? "la transición no está habilitada en la máquina de estados"}.")
    {
        Origen = origen;
        Destino = destino;
    }

    /// <summary>Estado en el que estaba la solicitud cuando se intentó el cambio.</summary>
    public EstadoSolicitudPermiso Origen { get; }

    /// <summary>Estado al que se quería pasar.</summary>
    public EstadoSolicitudPermiso Destino { get; }
}
