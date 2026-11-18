using Core.ControlAcceso.Application.Modelos;
using Core.ControlAcceso.Application.Servicios;
using Core.ControlAcceso.Domain.Entidades;
using Core.ControlAcceso.Domain.Enums;
using Core.ControlAcceso.Domain.Excepciones;
using Core.ControlAcceso.Infrastructura.Persistence;
using Core.ControlAcceso.Infrastructura.Repositorios;
using Microsoft.EntityFrameworkCore;
using Tests.Core.Dobles;
using Xunit;

namespace Tests.Core;

/// <summary>
/// C-17 (RF-NOT-11): consulta administrativa de la cola de correos.
/// </summary>
public class RF_NOT_11_ConsultaCorreosEnColaTests
{
    [Fact]
    public async Task RF_NOT_11_Listar_FiltraPorEstadoYDevuelveSinCuerpoDeRecienteAAntiguo()
    {
        // C-17: filtro por estado, sin cuerpo en el resultado, del más reciente al más antiguo.
        // Preparar
        var (unidad, contexto) = CrearUnidad();
        contexto.CorreosEnCola.AddRange(
            CrearCorreo(1, EstadoCorreo.Pendiente, new DateTime(2026, 11, 18, 8, 0, 0, DateTimeKind.Utc)),
            CrearCorreo(2, EstadoCorreo.Fallido, new DateTime(2026, 11, 18, 9, 0, 0, DateTimeKind.Utc), intentos: 3, ultimoError: "Error"),
            CrearCorreo(3, EstadoCorreo.Enviado, new DateTime(2026, 11, 18, 10, 0, 0, DateTimeKind.Utc), intentos: 1),
            CrearCorreo(4, EstadoCorreo.Fallido, new DateTime(2026, 11, 18, 11, 0, 0, DateTimeKind.Utc), intentos: 3, ultimoError: "Otro"));
        await contexto.SaveChangesAsync();
        var servicio = new ConsultaCorreosEnColaServicio(unidad);

        // Actuar
        var fallidos = await servicio.ListarAsync("fallido", CancellationToken.None);
        var todos = await servicio.ListarAsync(null, CancellationToken.None);

        // Verificar
        Assert.Equal(2, fallidos.Count);
        Assert.All(fallidos, c => Assert.Equal(EstadoCorreo.Fallido, c.Estado));
        Assert.Equal([4, 2], fallidos.Select(c => c.Id).ToList());
        Assert.All(fallidos, c => Assert.Equal(typeof(CorreoEnColaResumen), c.GetType()));
        var sinCuerpo = typeof(CorreoEnColaResumen).GetProperty("Cuerpo");
        Assert.Null(sinCuerpo);
        Assert.Equal([4, 3, 2, 1], todos.Select(c => c.Id).ToList());
        Assert.Equal(DateTimeKind.Utc, fallidos[0].FechaCreacion.Kind);
    }

    [Fact]
    public async Task RF_NOT_11_EstadoInvalido_LanzaErrorControlado400()
    {
        // C-17: estado inválido → error controlado.
        // Preparar
        var (unidad, _) = CrearUnidad();
        var servicio = new ConsultaCorreosEnColaServicio(unidad);

        // Actuar
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(
            () => servicio.ListarAsync("desconocido", CancellationToken.None));

        // Verificar
        Assert.Equal(400, excepcion.CodigoHttp);
        Assert.Equal(
            "El estado indicado no es válido. Valores permitidos: Pendiente, Enviando, Enviado, Fallido.",
            excepcion.Message);
    }

    private static (CoreUnidadDeTrabajo Unidad, CoreDbContext Contexto) CrearUnidad()
    {
        var reloj = new TimeProviderFijo(new DateTimeOffset(2026, 11, 18, 12, 0, 0, TimeSpan.Zero));
        var opciones = new DbContextOptionsBuilder<CoreDbContext>()
            .UseInMemoryDatabase($"correos-consulta-{Guid.NewGuid():N}")
            .Options;
        var contexto = new CoreDbContext(opciones, reloj);
        var unidad = new CoreUnidadDeTrabajo(contexto);
        return (unidad, contexto);
    }

    private static CorreoEnCola CrearCorreo(
        int id,
        EstadoCorreo estado,
        DateTime fechaCreacion,
        int intentos = 0,
        string? ultimoError = null)
        => new()
        {
            Id = id,
            Destinatario = $"destino{id}@ejemplo.com",
            Asunto = $"Asunto {id}",
            Cuerpo = $"Cuerpo secreto {id}",
            Estado = estado,
            Intentos = intentos,
            FechaCreacion = fechaCreacion,
            UltimoError = ultimoError,
            FechaUltimoIntento = intentos > 0 ? fechaCreacion : null,
            FechaEnvio = estado == EstadoCorreo.Enviado ? fechaCreacion : null,
        };
}
