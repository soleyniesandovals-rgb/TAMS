using Tams.Negocio.Domain;
using Tams.Negocio.Domain.Enums;
using Xunit;

namespace Tests.Negocio;

/// <summary>
/// RF-NEG-03: las cinco transiciones permitidas de la máquina de estados terminan en el
/// estado de destino cuando se cumplen sus condiciones de puntaje.
/// </summary>
public class TransicionesPermitidasTests
{
    public static TheoryData<decimal> PuntajesValidosParaEnviar => new() { 0m, 50m, 100m };

    [Theory]
    [MemberData(nameof(PuntajesValidosParaEnviar))]
    public void Transicion_Borrador_a_Enviada_conPuntajeValido_cambiaElEstado(decimal puntaje)
    {
        // Preparar
        var calificacion = CalificacionBuilder.Crear(EstadoCalificacion.Borrador, puntaje);

        // Actuar
        calificacion.CambiarEstado(EstadoCalificacion.Enviada, ReglasCalificacion.MinimoAprobacion);

        // Verificar
        Assert.Equal(EstadoCalificacion.Enviada, calificacion.Estado);
    }

    [Fact]
    public void Transicion_Enviada_a_Publicada_cambiaElEstado()
    {
        // Preparar
        var calificacion = CalificacionBuilder.Crear(EstadoCalificacion.Enviada);

        // Actuar
        calificacion.CambiarEstado(EstadoCalificacion.Publicada, ReglasCalificacion.MinimoAprobacion);

        // Verificar
        Assert.Equal(EstadoCalificacion.Publicada, calificacion.Estado);
    }

    [Fact]
    public void Transicion_Publicada_a_EnRecuperacion_conPuntajeBajoElMinimo_cambiaElEstado()
    {
        // Preparar
        var calificacion = CalificacionBuilder.Crear(EstadoCalificacion.Publicada, 69.99m);

        // Actuar
        calificacion.CambiarEstado(EstadoCalificacion.EnRecuperacion, ReglasCalificacion.MinimoAprobacion);

        // Verificar
        Assert.Equal(EstadoCalificacion.EnRecuperacion, calificacion.Estado);
    }

    [Fact]
    public void Transicion_Publicada_a_Cerrada_conPuntajeEnElMinimo_cambiaElEstado()
    {
        // Preparar
        var calificacion = CalificacionBuilder.Crear(EstadoCalificacion.Publicada, 70m);

        // Actuar
        calificacion.CambiarEstado(EstadoCalificacion.Cerrada, ReglasCalificacion.MinimoAprobacion);

        // Verificar
        Assert.Equal(EstadoCalificacion.Cerrada, calificacion.Estado);
    }

    [Fact]
    public void Transicion_EnRecuperacion_a_Cerrada_conPuntajeRecuperacion_cambiaElEstado()
    {
        // Preparar
        var calificacion = CalificacionBuilder.Crear(EstadoCalificacion.EnRecuperacion, 60m, 80m);

        // Actuar
        calificacion.CambiarEstado(EstadoCalificacion.Cerrada, ReglasCalificacion.MinimoAprobacion);

        // Verificar
        Assert.Equal(EstadoCalificacion.Cerrada, calificacion.Estado);
    }
}
