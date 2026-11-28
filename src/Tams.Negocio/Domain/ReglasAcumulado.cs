namespace Tams.Negocio.Domain;

/// <summary>
/// Regla única del acumulado por estudiante (RF-NEG, RD-02). Es pura: no depende
/// de EF Core ni de servicios, y la usan todas las consultas que necesiten el
/// acumulado, sea de periodos o de resultados de aprendizaje.
/// </summary>
public static class ReglasAcumulado
{
    /// <summary>
    /// Puntaje efectivo = <paramref name="puntajeRecuperacion"/> si tiene valor;
    /// si no, <paramref name="puntajeObtenido"/>. Cuentan todas las calificaciones
    /// registradas, sin importar su estado.
    /// </summary>
    public static decimal PuntajeEfectivo(
        decimal puntajeObtenido,
        decimal? puntajeRecuperacion)
        => puntajeRecuperacion ?? puntajeObtenido;

    /// <summary>
    /// Materia básica: acumulado = Σ(puntajes efectivos) / cantidad de periodos
    /// con nota. Los periodos sin nota no se incluyen en
    /// <paramref name="puntajesEfectivos"/>. Se redondea a 2 decimales con
    /// <see cref="MidpointRounding.AwayFromZero"/>; sin notas devuelve null.
    /// </summary>
    public static decimal? AcumuladoPeriodos(IEnumerable<decimal> puntajesEfectivos)
    {
        var notas = puntajesEfectivos.ToList();

        return notas.Count == 0
            ? null
            : Math.Round(notas.Average(), 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Materia técnica: acumulado = Σ(puntaje efectivo del RA × puntos del RA) / 100,
    /// contando solo los RA con nota. Es decir, puntos acumulados sobre 100;
    /// por ejemplo, RA1 = 80 con 34 puntos aporta 27,2. Se redondea a 2 decimales
    /// con <see cref="MidpointRounding.AwayFromZero"/>; sin notas devuelve null.
    /// </summary>
    public static decimal? AcumuladoRa(
        IEnumerable<(decimal PuntajeEfectivo, int Puntos)> notasRa)
    {
        var notas = notasRa.ToList();

        return notas.Count == 0
            ? null
            : Math.Round(
                notas.Sum(n => n.PuntajeEfectivo * n.Puntos) / 100m,
                2,
                MidpointRounding.AwayFromZero);
    }
}
