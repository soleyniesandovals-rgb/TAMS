using Tams.Negocio.Domain.Entidades;
using Tams.Negocio.Domain.Enums;
using Tams.Negocio.Domain.Excepciones;

namespace Tams.Negocio.Domain.Maquinas;

/// <summary>
/// Máquina de estados de <see cref="Calificacion"/> (RF-NEG-03, RF-NEG-04, RF-NEG-05).
///
/// RD-04: las transiciones permitidas se declaran en un único lugar, este mapa. Ningún
/// servicio ni controlador vuelve a listarlas, así que no puede haber dos listas que
/// se contradigan.
///
/// El mapa es exhaustivo: todo par (origen, destino) que no aparezca en
/// <see cref="PermitidosDesde"/> está prohibido. En particular, una calificación
/// publicada no vuelve a borrador (no se "despublica") y de
/// <see cref="EstadoCalificacion.Cerrada"/> no sale ninguna transición, porque es terminal.
/// La tabla completa, con quién ejecuta cada paso y su condición, está en
/// docs/maquina-de-estados.md.
/// </summary>
public static class TransicionesCalificacion
{
    /// <summary>Estados alcanzables desde cada estado. Única fuente de verdad (RD-04).</summary>
    private static readonly IReadOnlyDictionary<EstadoCalificacion, IReadOnlyCollection<EstadoCalificacion>> Mapa =
        new Dictionary<EstadoCalificacion, IReadOnlyCollection<EstadoCalificacion>>
        {
            // El docente envía lo que acaba de calificar.
            [EstadoCalificacion.Borrador] = [EstadoCalificacion.Enviada],

            // Lo enviado se publica para que el estudiante lo vea.
            [EstadoCalificacion.Enviada] = [EstadoCalificacion.Publicada],

            // Desde Publicada hay dos salidas excluyentes según el puntaje: si no alcanzó
            // el mínimo de aprobación pasa a recuperación; si lo alcanzó, se cierra directo.
            [EstadoCalificacion.Publicada] =
            [
                EstadoCalificacion.EnRecuperacion,
                EstadoCalificacion.Cerrada,
            ],

            // Con el resultado de la recuperación registrado, la calificación se cierra.
            [EstadoCalificacion.EnRecuperacion] = [EstadoCalificacion.Cerrada],

            // Estado terminal (RF-NEG-05): su lista de destinos está vacía a propósito.
            [EstadoCalificacion.Cerrada] = [],
        };

    /// <summary>Estados alcanzables desde <paramref name="estado"/>; lista vacía si es terminal.</summary>
    public static IReadOnlyCollection<EstadoCalificacion> PermitidosDesde(EstadoCalificacion estado) =>
        Mapa.TryGetValue(estado, out var permitidos)
            ? permitidos
            : Array.Empty<EstadoCalificacion>();

    /// <summary>Un estado es terminal cuando de él no sale ninguna transición (RF-NEG-05).</summary>
    public static bool EsTerminal(EstadoCalificacion estado) => PermitidosDesde(estado).Count == 0;

    /// <summary>
    /// Indica si la transición está habilitada en la máquina de estados, sin mirar todavía
    /// las condiciones de puntaje. Publicada → Borrador devuelve false aquí (RF-NEG-04).
    /// </summary>
    public static bool EsPermitida(EstadoCalificacion origen, EstadoCalificacion destino) =>
        PermitidosDesde(origen).Contains(destino);

    /// <summary>
    /// Exige una transición habilitada en la máquina de estados y lanza
    /// <see cref="TransicionNoPermitidaExcepcion"/> si no lo está (RF-NEG-04, RF-NEG-05).
    /// No evalúa las condiciones de puntaje; para eso está la sobrecarga con puntaje.
    /// </summary>
    public static void Exigir(EstadoCalificacion origen, EstadoCalificacion destino)
    {
        if (!EsPermitida(origen, destino))
        {
            throw new TransicionNoPermitidaExcepcion(origen, destino);
        }
    }

    /// <summary>
    /// Condición de negocio de la transición cuando depende del puntaje (columna "Condición"
    /// de docs/maquina-de-estados.md). El mínimo de aprobación lo pasa el llamador porque
    /// el sistema todavía no tiene dónde configurarlo.
    /// </summary>
    public static bool CumpleCondicion(
        EstadoCalificacion origen,
        EstadoCalificacion destino,
        decimal puntaje,
        decimal minimoAprobacion,
        decimal? puntajeRecuperacion = null) =>
        (origen, destino) switch
        {
            // A recuperación solo lo que no alcanzó el mínimo de aprobación.
            (EstadoCalificacion.Publicada, EstadoCalificacion.EnRecuperacion) => puntaje < minimoAprobacion,

            // Cierre directo, sin recuperación, solo para lo que ya aprobó.
            (EstadoCalificacion.Publicada, EstadoCalificacion.Cerrada) => puntaje >= minimoAprobacion,

            // No se cierra una recuperación sin resultado registrado.
            (EstadoCalificacion.EnRecuperacion, EstadoCalificacion.Cerrada) => puntajeRecuperacion.HasValue,

            // Los demás pasos no dependen del puntaje.
            _ => true,
        };

    /// <summary>
    /// Exige la transición completa: primero que esté habilitada en la máquina de estados
    /// y luego que se cumpla su condición de puntaje.
    /// </summary>
    public static void Exigir(
        EstadoCalificacion origen,
        EstadoCalificacion destino,
        decimal puntaje,
        decimal minimoAprobacion,
        decimal? puntajeRecuperacion = null)
    {
        Exigir(origen, destino);

        if (!CumpleCondicion(origen, destino, puntaje, minimoAprobacion, puntajeRecuperacion))
        {
            throw new TransicionNoPermitidaExcepcion(origen, destino, "la condición de puntaje no se cumple");
        }
    }
}