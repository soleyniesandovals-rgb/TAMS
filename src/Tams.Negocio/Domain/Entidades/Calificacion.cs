using Tams.Negocio.Domain.Enums;
using Tams.Negocio.Domain.Interfaces;
using Tams.Negocio.Domain.Maquinas;

namespace Tams.Negocio.Domain.Entidades;

/// <summary>
/// Calificación de un estudiante en una materia.
/// Puede ser por periodo o por RA. Su <see cref="Estado"/> avanza según la máquina de
/// estados de <see cref="TransicionesCalificacion"/>, que es la única que declara las
/// transiciones permitidas (RD-04).
/// </summary>
public class Calificacion : ICreacionAuditable
{
    public int Id { get; set; }

    public int EstudianteId { get; set; }

    public int MateriaId { get; set; }

    public int AnioEscolarId { get; set; }

    public TipoEvaluacion TipoEvaluacion { get; set; }

    /// <summary>Periodo evaluado cuando <see cref="TipoEvaluacion"/> es <see cref="TipoEvaluacion.Periodo"/>.</summary>
    public int? Periodo { get; set; }

    /// <summary>Número de RA evaluado cuando <see cref="TipoEvaluacion"/> es <see cref="TipoEvaluacion.RA"/>.</summary>
    public int? NumeroRA { get; set; }

    public decimal PuntajeObtenido { get; set; }

    public decimal? PuntajeRecuperacion { get; set; }

    public EstadoCalificacion Estado { get; set; }

    public DateTime FechaCreacion { get; set; }

    public Estudiante Estudiante { get; set; } = null!;

    public Materia Materia { get; set; } = null!;

    public AnioEscolar AnioEscolar { get; set; } = null!;

    /// <summary>
    /// Lleva la calificación a <paramref name="destino"/> si la máquina de estados lo permite
    /// (RF-NEG-03, RF-NEG-04). No modifica ningún puntaje: solo valida la transición con
    /// <see cref="TransicionesCalificacion"/> y actualiza <see cref="Estado"/>. Si la
    /// transición no está habilitada, o no cumple la condición de puntaje, lanza
    /// <c>TransicionNoPermitidaExcepcion</c>.
    /// </summary>
    public void CambiarEstado(EstadoCalificacion destino, decimal minimoAprobacion)
    {
        TransicionesCalificacion.Exigir(Estado, destino, PuntajeObtenido, minimoAprobacion, PuntajeRecuperacion);
        Estado = destino;
    }
}