using Core.ControlAcceso.Application.Servicios;
using Core.ControlAcceso.Domain.Entidades;
using Core.ControlAcceso.Domain.Enums;
using Core.ControlAcceso.Domain.Excepciones;
using Core.ControlAcceso.Infrastructura.Persistence;
using Core.ControlAcceso.Infrastructura.Repositorios;
using Microsoft.EntityFrameworkCore;
using Tests.Core.Dobles;

namespace Tests.Core;

/// <summary>
/// A3-H2 (C-17, RD-07): el filtro de estado de la cola solo acepta los NOMBRES
/// Pendiente, Enviando, Enviado y Fallido; los números y las combinaciones con
/// comas que acepta Enum.TryParse responden 400 controlado. Sin levantar la
/// aplicación ni usar SQL Server (RD-12).
/// </summary>
public class A3_H2_ConsultaCorreosEnColaEstadoPorNombreTests
{
    [Theory]
    [InlineData("0")]
    [InlineData("1")]
    [InlineData("99")]
    [InlineData("Pendiente,Enviado")]
    [InlineData("Perdido")]
    public async Task A3_H2_EstadoQueNoEsUnNombre_DevuelveErrorControlado400(string estado)
    {
        // Preparar: un servicio sobre una base vacía.
        await using var contexto = CrearContexto();
        var servicio = new ConsultaCorreosEnColaServicio(new CoreUnidadDeTrabajo(contexto));

        // Actuar: se filtra con un número, una combinación o un nombre inexistente.
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(
            () => servicio.ListarAsync(estado, CancellationToken.None));

        // Verificar: el mismo 400 controlado de siempre, sin aceptar el valor.
        Assert.Equal(400, excepcion.CodigoHttp);
        Assert.Equal(
            "El estado indicado no es válido. Valores permitidos: Pendiente, Enviando, Enviado, Fallido.",
            excepcion.Message);
    }

    [Fact]
    public async Task A3_H2_EstadoFallidoEnMinusculas_FiltraSoloLosFallidos()
    {
        // Preparar: correos en los tres estados.
        await using var contexto = CrearContexto();
        contexto.CorreosEnCola.AddRange(
            CrearCorreo(1, EstadoCorreo.Pendiente),
            CrearCorreo(2, EstadoCorreo.Fallido),
            CrearCorreo(3, EstadoCorreo.Enviado));
        await contexto.SaveChangesAsync();
        var servicio = new ConsultaCorreosEnColaServicio(new CoreUnidadDeTrabajo(contexto));

        // Actuar: se filtra con el nombre en minúsculas.
        var correos = await servicio.ListarAsync("fallido", CancellationToken.None);

        // Verificar: solo el Fallido, sin distinguir mayúsculas.
        var correo = Assert.Single(correos);
        Assert.Equal(2, correo.Id);
        Assert.Equal(EstadoCorreo.Fallido, correo.Estado);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task A3_H2_EstadoAusenteOVacio_SigueSignificandoSinFiltro(string? estado)
    {
        // Preparar: correos en dos estados distintos.
        await using var contexto = CrearContexto();
        contexto.CorreosEnCola.AddRange(
            CrearCorreo(1, EstadoCorreo.Pendiente),
            CrearCorreo(2, EstadoCorreo.Enviado));
        await contexto.SaveChangesAsync();
        var servicio = new ConsultaCorreosEnColaServicio(new CoreUnidadDeTrabajo(contexto));

        // Actuar: no se indica estado.
        var correos = await servicio.ListarAsync(estado, CancellationToken.None);

        // Verificar: se devuelven todos, sin filtro.
        Assert.Equal([2, 1], correos.Select(c => c.Id).ToList());
    }

    private static CoreDbContext CrearContexto()
    {
        var reloj = new TimeProviderFijo(new DateTimeOffset(2026, 11, 18, 12, 0, 0, TimeSpan.Zero));
        var opciones = new DbContextOptionsBuilder<CoreDbContext>()
            .UseInMemoryDatabase($"a3-h2-correos-{Guid.NewGuid():N}")
            .Options;
        return new CoreDbContext(opciones, reloj);
    }

    private static CorreoEnCola CrearCorreo(int id, EstadoCorreo estado)
        => new()
        {
            Id = id,
            Destinatario = $"destino{id}@ejemplo.com",
            Asunto = $"Asunto {id}",
            Cuerpo = $"Cuerpo {id}",
            Estado = estado,
            FechaCreacion = new DateTime(2026, 11, 18, 8 + id, 0, 0, DateTimeKind.Utc),
        };
}
