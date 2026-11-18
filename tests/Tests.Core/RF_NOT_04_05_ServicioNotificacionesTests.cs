using Core.ControlAcceso.Domain.Excepciones;
using Core.ControlAcceso.Infrastructura.Persistence;
using Core.Notificaciones.Domain.Entidades;
using Core.Notificaciones.Domain.Enums;
using Tests.Core.Dobles;
using Xunit;

namespace Tests.Core;

/// <summary>
/// C-06 a C-08 (RF-NOT-04, RF-NOT-05): bandeja propia y marcado de lectura.
/// </summary>
public class ServicioNotificacionesTests
{
    [Fact]
    public async Task RF_NOT_04_ListarPropias_DevuelveSoloLasDelUsuarioDeMayorAMenor()
    {
        // Preparar: tres avisos propios en instantes distintos y uno de otro usuario.
        var contexto = ContextoNotificaciones.Crear();
        await ContextoNotificaciones.SembrarUsuarioAsync(contexto.Contexto, 1, "Ana", "ana@ejemplo.com");
        await ContextoNotificaciones.SembrarUsuarioAsync(contexto.Contexto, 2, "Luis", "luis@ejemplo.com");

        contexto.Reloj.Instante = new DateTimeOffset(2026, 10, 17, 10, 0, 0, TimeSpan.Zero);
        var antigua = await SembrarAsync(contexto.Contexto, 1, "Antigua");
        contexto.Reloj.Instante = new DateTimeOffset(2026, 10, 17, 11, 0, 0, TimeSpan.Zero);
        var intermedia = await SembrarAsync(contexto.Contexto, 1, "Intermedia");
        contexto.Reloj.Instante = new DateTimeOffset(2026, 10, 17, 12, 0, 0, TimeSpan.Zero);
        var reciente = await SembrarAsync(contexto.Contexto, 1, "Reciente");
        await SembrarAsync(contexto.Contexto, 2, "Ajena");

        // Actuar: el usuario 1 consulta su bandeja.
        var propias = await contexto.Servicio.ListarPropiasAsync(1, CancellationToken.None);

        // Verificar: solo las suyas, ordenadas de la más reciente a la más antigua.
        Assert.Equal(3, propias.Count);
        Assert.All(propias, notificacion => Assert.Equal(1, notificacion.UsuarioId));
        Assert.Equal([reciente.Id, intermedia.Id, antigua.Id], propias.Select(n => n.Id).ToList());
        Assert.All(propias, notificacion => Assert.False(notificacion.Leida));
    }

    [Fact]
    public async Task RF_NOT_05_MarcarLeida_RegistraFechaDelRelojYNoLaReescribe()
    {
        // Preparar: una not propia sin leer y un reloj controlado.
        var contexto = ContextoNotificaciones.Crear();
        await ContextoNotificaciones.SembrarUsuarioAsync(contexto.Contexto, 1, "Ana", "ana@ejemplo.com");
        var notificacion = await SembrarAsync(contexto.Contexto, 1, "Aviso");
        var primerInstante = contexto.Reloj.Instante.UtcDateTime;

        // Actuar: se marca como leída y se vuelve a intentar con el reloj avanzado.
        var primera = await contexto.Servicio.MarcarLeidaAsync(1, notificacion.Id, CancellationToken.None);
        contexto.Reloj.Instante = contexto.Reloj.Instante.AddHours(1);
        var segunda = await contexto.Servicio.MarcarLeidaAsync(1, notificacion.Id, CancellationToken.None);

        // Verificar: queda leída y la fecha de la primera lectura no cambia.
        Assert.True(primera.Leida);
        Assert.Equal(primerInstante, primera.FechaLectura);
        Assert.True(segunda.Leida);
        Assert.Equal(primerInstante, segunda.FechaLectura);
    }

    [Fact]
    public async Task RF_NOT_05_MarcarLeida_DeOtroUsuarioOInexistente_Rechaza404SinModificar()
    {
        // Preparar: una notificación ajena sin leer y un id que no existe.
        var contexto = ContextoNotificaciones.Crear();
        await ContextoNotificaciones.SembrarUsuarioAsync(contexto.Contexto, 1, "Ana", "ana@ejemplo.com");
        await ContextoNotificaciones.SembrarUsuarioAsync(contexto.Contexto, 2, "Luis", "luis@ejemplo.com");
        var ajena = await SembrarAsync(contexto.Contexto, 2, "Ajena");

        // Actuar: el usuario 1 intenta marcar la ajena y después un id inexistente.
        var excepcionAjena = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(
            () => contexto.Servicio.MarcarLeidaAsync(1, ajena.Id, CancellationToken.None));
        var excepcionInexistente = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(
            () => contexto.Servicio.MarcarLeidaAsync(1, 9999, CancellationToken.None));

        // Verificar: ambos casos dan 404 controlado y la ajena sigue sin leer.
        Assert.Equal(404, excepcionAjena.CodigoHttp);
        Assert.Equal("La notificación no existe.", excepcionAjena.Message);
        Assert.Equal(404, excepcionInexistente.CodigoHttp);
        Assert.Equal("La notificación no existe.", excepcionInexistente.Message);
        Assert.False(ajena.Leida);
        Assert.Null(ajena.FechaLectura);
    }

    private static async Task<Notificacion> SembrarAsync(
        CoreDbContext contexto,
        int usuarioId,
        string titulo)
    {
        var notificacion = new Notificacion
        {
            UsuarioId = usuarioId,
            Tipo = TipoNotificacion.SolicitudCreada,
            Titulo = titulo,
            Mensaje = $"Mensaje de {titulo}",
        };

        contexto.Notificaciones.Add(notificacion);
        await contexto.SaveChangesAsync();

        return notificacion;
    }
}
