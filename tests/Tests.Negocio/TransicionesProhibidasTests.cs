using Tams.Negocio.Domain;
using Tams.Negocio.Domain.Enums;
using Tams.Negocio.Domain.Excepciones;
using Tams.Negocio.Domain.Maquinas;
using Xunit;

namespace Tests.Negocio;

/// <summary>
/// RF-NEG-04 y RF-NEG-05: las transiciones prohibidas no cambian el estado y Cerrada es
/// terminal. La teoría exhaustiva genera todos los pares no permitidos desde el enum.
/// </summary>
public class TransicionesProhibidasTests
{
    private static readonly HashSet<(EstadoCalificacion Origen, EstadoCalificacion Destino)> Permitidas =
    [
        (EstadoCalificacion.Borrador, EstadoCalificacion.Enviada),
        (EstadoCalificacion.Enviada, EstadoCalificacion.Publicada),
        (EstadoCalificacion.Publicada, EstadoCalificacion.EnRecuperacion),
        (EstadoCalificacion.Publicada, EstadoCalificacion.Cerrada),
        (EstadoCalificacion.EnRecuperacion, EstadoCalificacion.Cerrada),
    ];

    public static IEnumerable<object[]> ParesNoPermitidos()
    {
        foreach (var origen in Enum.GetValues<EstadoCalificacion>())
        {
            foreach (var destino in Enum.GetValues<EstadoCalificacion>())
            {
                if (!Permitidas.Contains((origen, destino)))
                {
                    yield return new object[] { origen, destino };
                }
            }
        }
    }

    public static IEnumerable<object[]> TodosLosEstados()
    {
        foreach (var estado in Enum.GetValues<EstadoCalificacion>())
        {
            yield return new object[] { estado };
        }
    }

    [Fact]
    public void Transicion_Publicada_a_Borrador_lanzaExcepcion()
    {
        // Preparar
        var calificacion = CalificacionBuilder.Crear(EstadoCalificacion.Publicada);

        // Actuar
        var excepcion = Assert.Throws<TransicionNoPermitidaExcepcion>(() =>
            calificacion.CambiarEstado(EstadoCalificacion.Borrador, ReglasCalificacion.MinimoAprobacion));

        // Verificar
        Assert.Equal(EstadoCalificacion.Publicada, excepcion.Origen);
        Assert.Equal(EstadoCalificacion.Borrador, excepcion.Destino);
        Assert.Equal(EstadoCalificacion.Publicada, calificacion.Estado);
    }

    [Theory]
    [MemberData(nameof(TodosLosEstados))]
    public void Transicion_Cerrada_a_CualquierEstado_lanzaExcepcion(EstadoCalificacion destino)
    {
        // Preparar
        var calificacion = CalificacionBuilder.Crear(EstadoCalificacion.Cerrada);

        // Actuar
        var excepcion = Assert.Throws<TransicionNoPermitidaExcepcion>(() =>
            calificacion.CambiarEstado(destino, ReglasCalificacion.MinimoAprobacion));

        // Verificar
        Assert.Equal(EstadoCalificacion.Cerrada, excepcion.Origen);
        Assert.Equal(destino, excepcion.Destino);
        Assert.Equal(EstadoCalificacion.Cerrada, calificacion.Estado);
    }

    [Fact]
    public void Estado_Cerrada_esTerminal()
    {
        // Preparar
        var estado = EstadoCalificacion.Cerrada;

        // Actuar
        var esTerminal = TransicionesCalificacion.EsTerminal(estado);
        var permitidos = TransicionesCalificacion.PermitidosDesde(estado);

        // Verificar
        Assert.True(esTerminal);
        Assert.Empty(permitidos);
    }

    [Theory]
    [MemberData(nameof(ParesNoPermitidos))]
    public void Transicion_EntreCualquierParNoPermitido_lanzaExcepcionYNoCambiaElEstado(
        EstadoCalificacion origen,
        EstadoCalificacion destino)
    {
        // Preparar
        var calificacion = CalificacionBuilder.Crear(origen);

        // Actuar
        var excepcion = Assert.Throws<TransicionNoPermitidaExcepcion>(() =>
            calificacion.CambiarEstado(destino, ReglasCalificacion.MinimoAprobacion));

        // Verificar
        Assert.Equal(origen, excepcion.Origen);
        Assert.Equal(destino, excepcion.Destino);
        Assert.Equal(origen, calificacion.Estado);
    }
}
