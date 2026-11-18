using System.Globalization;
using Core.ControlAcceso.Domain.Excepciones;

namespace Core.Reportes.Application;

/// <summary>
/// Rango de fechas de los reportes (RF-REP-01, RF-REP-02, RF-REP-05, RD-07).
/// Convierte los extremos recibidos como texto <c>AAAA-MM-DD</c> con cultura invariante
/// y los valida en un solo lugar, para que ambos reportes se comporten igual.
/// Los dos extremos son INCLUIDOS: el negocio compara fechas con <c>&gt;=</c> y
/// <c>&lt;=</c>; el Core usa <c>&gt;= desde</c> y <c>&lt; (hasta + 1 día)</c> para
/// contar también cualquier hora del último día (C-24).
/// </summary>
public sealed class RangoFechasReporte
{
    private RangoFechasReporte(DateOnly desde, DateOnly hasta)
    {
        Desde = desde;
        Hasta = hasta;
    }

    /// <summary>Primer día del rango, incluido.</summary>
    public DateOnly Desde { get; }

    /// <summary>Último día del rango, incluido.</summary>
    public DateOnly Hasta { get; }

    /// <summary>Límite inferior UTC para comparar columnas <c>datetime2</c>.</summary>
    public DateTime DesdeUtc => Desde.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

    /// <summary>
    /// Límite superior EXCLUSIVO UTC: la medianoche del día siguiente a <see cref="Hasta"/>.
    /// Así se incluyen las solicitudes creadas hasta las 23:59:59 del último día (C-24).
    /// </summary>
    public DateTime HastaExclusivoUtc => Hasta.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

    /// <summary>
    /// Parsea y valida los extremos. Cualquier entrada inválida produce una
    /// <see cref="ReglaNegocioExcepcion"/> con código 400 y un mensaje controlado (RD-07, RD-08).
    /// </summary>
    public static RangoFechasReporte Crear(string? desde, string? hasta)
    {
        var desdeFecha = Parsear(desde, nameof(desde));
        var hastaFecha = Parsear(hasta, nameof(hasta));

        if (desdeFecha > hastaFecha)
        {
            throw new ReglaNegocioExcepcion("desde no puede ser posterior a hasta.");
        }

        return new RangoFechasReporte(desdeFecha, hastaFecha);
    }

    private static DateOnly Parsear(string? valor, string nombre)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            throw new ReglaNegocioExcepcion($"{nombre} es obligatorio con formato AAAA-MM-DD.");
        }

        if (!DateOnly.TryParseExact(
                valor,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var fecha))
        {
            throw new ReglaNegocioExcepcion($"{nombre} debe tener el formato AAAA-MM-DD.");
        }

        return fecha;
    }
}
