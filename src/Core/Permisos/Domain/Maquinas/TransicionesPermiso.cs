using Core.Permisos.Domain.Enums;
using Core.Permisos.Domain.Excepciones;

namespace Core.Permisos.Domain.Maquinas;

/// <summary>
/// Máquina de estados de <c>SolicitudPermiso</c> (RF-GP-06).
///
/// RD-04: las transiciones permitidas se declaran en un único lugar, este mapa.
/// Ningún servicio ni controlador vuelve a listarlas, así que no puede haber dos
/// listas que se contradigan.
///
/// El mapa es exhaustivo: todo par (origen, destino) que no aparezca en
/// <see cref="PermitidosDesde"/> está prohibido. Una solicitud ya resuelta no se
/// vuelve a resolver (rechazada no pasa a aprobada) y <see cref="EstadoSolicitudPermiso.Rechazada"/>
/// y <see cref="EstadoSolicitudPermiso.Aplicada"/> son terminales.
/// </summary>
public static class TransicionesPermiso
{
    /// <summary>Estados alcanzables desde cada estado. Única fuente de verdad (RD-04).</summary>
    private static readonly IReadOnlyDictionary<EstadoSolicitudPermiso, IReadOnlyCollection<EstadoSolicitudPermiso>> Mapa =
        new Dictionary<EstadoSolicitudPermiso, IReadOnlyCollection<EstadoSolicitudPermiso>>
        {
            // La solicitud recién creada la resuelve un administrador: la aprueba o la rechaza.
            [EstadoSolicitudPermiso.Pendiente] =
            [
                EstadoSolicitudPermiso.Aprobada,
                EstadoSolicitudPermiso.Rechazada,
            ],

            // Aprobada se materializa otorgando el permiso al usuario (RF-GP-05).
            [EstadoSolicitudPermiso.Aprobada] = [EstadoSolicitudPermiso.Aplicada],

            // Estado terminal: su lista de destinos está vacía a propósito.
            [EstadoSolicitudPermiso.Rechazada] = [],

            // Estado terminal: su lista de destinos está vacía a propósito.
            [EstadoSolicitudPermiso.Aplicada] = [],
        };

    /// <summary>Estados alcanzables desde <paramref name="estado"/>; lista vacía si es terminal.</summary>
    public static IReadOnlyCollection<EstadoSolicitudPermiso> PermitidosDesde(EstadoSolicitudPermiso estado) =>
        Mapa.TryGetValue(estado, out var permitidos)
            ? permitidos
            : Array.Empty<EstadoSolicitudPermiso>();

    /// <summary>Un estado es terminal cuando de él no sale ninguna transición.</summary>
    public static bool EsTerminal(EstadoSolicitudPermiso estado) => PermitidosDesde(estado).Count == 0;

    /// <summary>
    /// Indica si la transición está habilitada en la máquina de estados. Equivalente al
    /// <c>EsPermitida</c> de <c>TransicionesCalificacion</c> (RD-04).
    /// </summary>
    public static bool EsValida(EstadoSolicitudPermiso origen, EstadoSolicitudPermiso destino) =>
        PermitidosDesde(origen).Contains(destino);

    /// <summary>
    /// Exige una transición habilitada en la máquina de estados y lanza
    /// <see cref="TransicionNoPermitidaExcepcion"/> si no lo está (RF-GP-06).
    /// </summary>
    public static void Exigir(EstadoSolicitudPermiso origen, EstadoSolicitudPermiso destino)
    {
        if (!EsValida(origen, destino))
        {
            throw new TransicionNoPermitidaExcepcion(origen, destino);
        }
    }
}
