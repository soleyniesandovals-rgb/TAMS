using Tams.Negocio.Domain.Enums;

namespace Tams.Negocio.Domain.Excepciones;

/// <summary>
/// Transición de estado que la máquina de estados no permite (RF-NEG-04, RF-NEG-05).
/// Es un error de negocio con mensaje controlado para el usuario final (RD-07, RD-08);
/// la capa que la reciba la traduce a respuesta HTTP (por ejemplo, 409 Conflict).
/// </summary>
public class TransicionNoPermitidaExcepcion : Exception
{
    public TransicionNoPermitidaExcepcion(EstadoCalificacion origen, EstadoCalificacion destino, string? motivo = null)
        : base($"No se permite pasar de {origen} a {destino}. Motivo: {motivo ?? "la transición no está habilitada en la máquina de estados"}.")
    {
        Origen = origen;
        Destino = destino;
    }

    /// <summary>Estado en el que estaba la calificación cuando se intentó el cambio.</summary>
    public EstadoCalificacion Origen { get; }

    /// <summary>Estado al que se quería pasar.</summary>
    public EstadoCalificacion Destino { get; }
}