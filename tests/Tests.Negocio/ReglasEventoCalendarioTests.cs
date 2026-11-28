using Tams.Negocio.Domain;
using Xunit;

namespace Tests.Negocio;

/// <summary>
/// RF-NEG (RD-02, RD-07): límites de la regla pura de eventos del calendario.
/// No usan base de datos ni reloj.
/// </summary>
public class ReglasEventoCalendarioTests
{
    private static readonly DateOnly Inicio = new(2026, 8, 1);
    private static readonly DateOnly Fin = new(2027, 6, 30);

    [Fact]
    public void RF_NEG_FechaEvento_EnFechaInicioYFechaFin_SeAcepta()
    {
        // Preparar: los dos extremos del año escolar, ambos incluidos.

        // Actuar: se evalúan las dos fechas límite con la regla pura.
        var inicioEsValido = ReglasEventoCalendario.EsFechaValida(Inicio, Inicio, Fin);
        var finEsValido = ReglasEventoCalendario.EsFechaValida(Fin, Inicio, Fin);

        // Verificar: ninguno de los extremos se rechaza.
        Assert.True(inicioEsValido);
        Assert.True(finEsValido);
    }

    [Fact]
    public void RF_NEG_FechaEvento_UnDiaAntesDelInicio_SeRechaza()
    {
        // Preparar: un día inmediatamente anterior al inicio del año escolar.
        var fecha = Inicio.AddDays(-1);

        // Actuar: se evalúa la fecha con la regla pura.
        var esValida = ReglasEventoCalendario.EsFechaValida(fecha, Inicio, Fin);

        // Verificar: la fecha queda fuera del año.
        Assert.False(esValida);
    }

    [Fact]
    public void RF_NEG_FechaEvento_UnDiaDespuesDelFin_SeRechaza()
    {
        // Preparar: un día inmediatamente posterior al fin del año escolar.
        var fecha = Fin.AddDays(1);

        // Actuar: se evalúa la fecha con la regla pura.
        var esValida = ReglasEventoCalendario.EsFechaValida(fecha, Inicio, Fin);

        // Verificar: la fecha queda fuera del año.
        Assert.False(esValida);
    }

    [Fact]
    public void RF_NEG_DescripcionEvento_VaciaYSoloEspacios_SeRechaza()
    {
        // Preparar: una descripción vacía y otra que solo contiene espacios.

        // Actuar: se normalizan y evalúan con la regla pura.
        var vacia = ReglasEventoCalendario.NormalizarDescripcion(string.Empty);
        var espacios = ReglasEventoCalendario.NormalizarDescripcion("   ");

        // Verificar: ninguna de las dos es aceptable.
        Assert.False(ReglasEventoCalendario.EsDescripcionValida(vacia));
        Assert.False(ReglasEventoCalendario.EsDescripcionValida(espacios));
    }

    [Fact]
    public void RF_NEG_DescripcionEvento_200Caracteres_SeAcepta()
    {
        // Preparar: una descripción de exactamente 200 caracteres.
        var descripcion = new string('a', ReglasEventoCalendario.LongitudMaximaDescripcion);

        // Actuar: se evalúa con la regla pura.
        var esValida = ReglasEventoCalendario.EsDescripcionValida(
            ReglasEventoCalendario.NormalizarDescripcion(descripcion));

        // Verificar: el límite está incluido.
        Assert.True(esValida);
    }

    [Fact]
    public void RF_NEG_DescripcionEvento_201Caracteres_SeRechaza()
    {
        // Preparar: una descripción de 201 caracteres.
        var descripcion = new string('a', ReglasEventoCalendario.LongitudMaximaDescripcion + 1);

        // Actuar: se evalúa con la regla pura.
        var esValida = ReglasEventoCalendario.EsDescripcionValida(
            ReglasEventoCalendario.NormalizarDescripcion(descripcion));

        // Verificar: supera el límite.
        Assert.False(esValida);
    }

    [Fact]
    public void RF_NEG_DescripcionEvento_ConEspaciosAlrededor_SeRecorta()
    {
        // Preparar: una descripción válida rodeada de espacios.
        const string descripcion = "   Entrega de notas   ";

        // Actuar: se normaliza con la regla pura.
        var normalizada = ReglasEventoCalendario.NormalizarDescripcion(descripcion);

        // Verificar: se guarda sin los espacios sobrantes y sigue siendo válida.
        Assert.Equal("Entrega de notas", normalizada);
        Assert.True(ReglasEventoCalendario.EsDescripcionValida(normalizada));
    }
}
