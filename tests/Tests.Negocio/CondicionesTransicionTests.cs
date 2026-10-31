using Tams.Negocio.Domain;
using Tams.Negocio.Domain.Enums;
using Tams.Negocio.Domain.Excepciones;
using Xunit;

namespace Tests.Negocio;

/// <summary>
/// RF-NEG-03: las condiciones de puntaje de las transiciones rechazan los valores fuera de
/// rango, en el mínimo de aprobación o sin el resultado de recuperación requerido.
/// </summary>
public class CondicionesTransicionTests
{
    public static TheoryData<decimal> PuntajesFueraDeRangoParaEnviar => new() { 100.01m, -1m };

    [Theory]
    [MemberData(nameof(PuntajesFueraDeRangoParaEnviar))]
    public void Transicion_Borrador_a_Enviada_conPuntajeFueraDeRango_lanzaExcepcion(decimal puntaje)
    {
        // Preparar
        var calificacion = CalificacionBuilder.Crear(EstadoCalificacion.Borrador, puntaje);

        // Actuar
        var excepcion = Assert.Throws<TransicionNoPermitidaExcepcion>(() =>
            calificacion.CambiarEstado(EstadoCalificacion.Enviada, ReglasCalificacion.MinimoAprobacion));

        // Verificar
        Assert.Equal(EstadoCalificacion.Borrador, excepcion.Origen);
        Assert.Equal(EstadoCalificacion.Enviada, excepcion.Destino);
        Assert.Equal(EstadoCalificacion.Borrador, calificacion.Estado);
    }

    [Fact]
    public void Transicion_Publicada_a_EnRecuperacion_conPuntajeEnElMinimo_lanzaExcepcion()
    {
        // Preparar
        var calificacion = CalificacionBuilder.Crear(EstadoCalificacion.Publicada, 70m);

        // Actuar
        var excepcion = Assert.Throws<TransicionNoPermitidaExcepcion>(() =>
            calificacion.CambiarEstado(EstadoCalificacion.EnRecuperacion, ReglasCalificacion.MinimoAprobacion));

        // Verificar
        Assert.Equal(EstadoCalificacion.Publicada, excepcion.Origen);
        Assert.Equal(EstadoCalificacion.EnRecuperacion, excepcion.Destino);
        Assert.Equal(EstadoCalificacion.Publicada, calificacion.Estado);
    }

    [Fact]
    public void Transicion_Publicada_a_Cerrada_conPuntajeBajoElMinimo_lanzaExcepcion()
    {
        // Preparar
        var calificacion = CalificacionBuilder.Crear(EstadoCalificacion.Publicada, 69.99m);

        // Actuar
        var excepcion = Assert.Throws<TransicionNoPermitidaExcepcion>(() =>
            calificacion.CambiarEstado(EstadoCalificacion.Cerrada, ReglasCalificacion.MinimoAprobacion));

        // Verificar
        Assert.Equal(EstadoCalificacion.Publicada, excepcion.Origen);
        Assert.Equal(EstadoCalificacion.Cerrada, excepcion.Destino);
        Assert.Equal(EstadoCalificacion.Publicada, calificacion.Estado);
    }

    [Fact]
    public void Transicion_EnRecuperacion_a_Cerrada_sinPuntajeRecuperacion_lanzaExcepcion()
    {
        // Preparar
        var calificacion = CalificacionBuilder.Crear(EstadoCalificacion.EnRecuperacion, 60m);

        // Actuar
        var excepcion = Assert.Throws<TransicionNoPermitidaExcepcion>(() =>
            calificacion.CambiarEstado(EstadoCalificacion.Cerrada, ReglasCalificacion.MinimoAprobacion));

        // Verificar
        Assert.Equal(EstadoCalificacion.EnRecuperacion, excepcion.Origen);
        Assert.Equal(EstadoCalificacion.Cerrada, excepcion.Destino);
        Assert.Equal(EstadoCalificacion.EnRecuperacion, calificacion.Estado);
    }

    [Fact]
    public void Transicion_EnRecuperacion_a_Cerrada_conPuntajeRecuperacionFueraDeRango_lanzaExcepcion()
    {
        // Preparar
        var calificacion = CalificacionBuilder.Crear(EstadoCalificacion.EnRecuperacion, 60m, 101m);

        // Actuar
        var excepcion = Assert.Throws<TransicionNoPermitidaExcepcion>(() =>
            calificacion.CambiarEstado(EstadoCalificacion.Cerrada, ReglasCalificacion.MinimoAprobacion));

        // Verificar
        Assert.Equal(EstadoCalificacion.EnRecuperacion, excepcion.Origen);
        Assert.Equal(EstadoCalificacion.Cerrada, excepcion.Destino);
        Assert.Equal(EstadoCalificacion.EnRecuperacion, calificacion.Estado);
    }
}
