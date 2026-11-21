using Core.Auditoria.Application.Servicios;
using Core.Auditoria.Domain.Enums;
using Tests.Core.Dobles;
using Xunit;

namespace Tests.Core;

/// <summary>
/// CA-01, CA-12: registro completo, fecha del reloj y un solo guardado (RF-AUD-01, RD-11).
/// </summary>
public class RegistradorAuditoriaTests
{
    [Fact]
    public async Task RF_AUD_01_Registrar_AgregaRegistroConTodosLosCamposYFechaUtcDelReloj()
    {
        // CA-01: exactamente un registro con todos los campos y fecha UTC del TimeProvider (RD-11).
        // Preparar: un reloj fijo con desplazamiento y una unidad de trabajo en memoria.
        var instante = new DateTimeOffset(2026, 11, 21, 15, 30, 45, TimeSpan.FromHours(-4));
        var reloj = new TimeProviderFijo(instante);
        var unidad = new UnidadDeTrabajoAuditoriaFalsa();
        var registrador = new RegistradorAuditoria(unidad, reloj);

        // Actuar: se registra una acción con valores anterior y nuevo.
        await registrador.RegistrarAsync(
            7,
            AccionAuditoria.AprobarSolicitud,
            EntidadAuditoria.SolicitudPermiso,
            42,
            "Pendiente",
            "Aplicada (Vacaciones)",
            CancellationToken.None);

        // Verificar: un solo registro con todos los campos y la fecha convertida a UTC.
        var registro = Assert.Single(unidad.Registros);
        Assert.Equal(7, registro.UsuarioId);
        Assert.Equal(AccionAuditoria.AprobarSolicitud, registro.Accion);
        Assert.Equal(EntidadAuditoria.SolicitudPermiso, registro.Entidad);
        Assert.Equal(42, registro.EntidadId);
        Assert.Equal(instante.UtcDateTime, registro.FechaHora);
        Assert.Equal(DateTimeKind.Utc, registro.FechaHora.Kind);
        Assert.Equal("Pendiente", registro.ValorAnterior);
        Assert.Equal("Aplicada (Vacaciones)", registro.ValorNuevo);
    }

    [Fact]
    public async Task RF_AUD_01_Registrar_NoGuardaCambiosPorSiMismo()
    {
        // CA-12: el registro va en el mismo guardado de la operación auditada; el registrador no guarda.
        // Preparar: unidad de trabajo que cuenta los guardados y registrador con reloj fijo.
        var unidad = new UnidadDeTrabajoAuditoriaFalsa();
        var registrador = new RegistradorAuditoria(unidad, new TimeProviderFijo(new DateTimeOffset(2026, 11, 21, 12, 0, 0, TimeSpan.Zero)));

        // Actuar: solo se registra; después la "operación auditada" guarda una vez.
        await registrador.RegistrarAsync(
            3,
            AccionAuditoria.SubirDocumento,
            EntidadAuditoria.Documento,
            9,
            null,
            "guia.pdf (PDF, 128 bytes)",
            CancellationToken.None);
        var guardadosTrasRegistrar = unidad.Guardados;
        await unidad.GuardarCambiosAsync(CancellationToken.None);

        // Verificar: el registrador no guardó y el registro sigue allí tras el guardado externo.
        Assert.Equal(0, guardadosTrasRegistrar);
        Assert.Equal(1, unidad.Guardados);
        Assert.Single(unidad.Registros);
    }

    [Fact]
    public async Task RF_AUD_01_Registrar_ValoresLargosSeRecortanYNulosSeConservan()
    {
        // CA-01: valores legibles de hasta 500 caracteres; null significa "sin valor" (RD-07).
        // Preparar: textos de 501, 700 y exactamente 500 caracteres, más una cadena vacía.
        var unidad = new UnidadDeTrabajoAuditoriaFalsa();
        var registrador = new RegistradorAuditoria(unidad, new TimeProviderFijo(new DateTimeOffset(2026, 11, 21, 12, 0, 0, TimeSpan.Zero)));
        var anteriorLargo = new string('A', 501);
        var nuevoLargo = new string('N', 700);
        var anteriorExacto = new string('E', 500);

        // Actuar: registrar los tres casos límite.
        await registrador.RegistrarAsync(1, AccionAuditoria.CambiarContrasena, EntidadAuditoria.Usuario, 3,
            anteriorLargo, null, CancellationToken.None);
        await registrador.RegistrarAsync(1, AccionAuditoria.RecuperarContrasena, EntidadAuditoria.Usuario, 3,
            null, nuevoLargo, CancellationToken.None);
        await registrador.RegistrarAsync(1, AccionAuditoria.RestablecerContrasena, EntidadAuditoria.Usuario, 3,
            anteriorExacto, string.Empty, CancellationToken.None);

        // Verificar: se recorta solo lo que pasa de 500; null y la longitud exacta se conservan.
        Assert.Equal(3, unidad.Registros.Count);
        Assert.Equal(new string('A', 500), unidad.Registros[0].ValorAnterior);
        Assert.Null(unidad.Registros[0].ValorNuevo);
        Assert.Null(unidad.Registros[1].ValorAnterior);
        Assert.Equal(new string('N', 500), unidad.Registros[1].ValorNuevo);
        Assert.Equal(anteriorExacto, unidad.Registros[2].ValorAnterior);
        Assert.Equal(string.Empty, unidad.Registros[2].ValorNuevo);
    }

    [Fact]
    public void RF_AUD_01_EnumsAuditoria_TienenLosNombresExactosDelCriterio()
    {
        // CA-01: los nombres de entidades y acciones son exactamente los del documento de criterios.
        // Preparar: la lista publicada en docs/criterios-auditoria.md.
        var accionesEsperadas = new[]
        {
            "AprobarSolicitud",
            "RechazarSolicitud",
            "SubirDocumento",
            "EliminarDocumento",
            "CambiarRol",
            "DesactivarUsuario",
            "ReactivarUsuario",
            "RestablecerContrasena",
            "RecuperarContrasena",
            "CambiarContrasena",
        };
        var entidadesEsperadas = new[] { "SolicitudPermiso", "Documento", "Usuario" };

        // Actuar: se leen los nombres de los enums del dominio.
        var acciones = Enum.GetNames<AccionAuditoria>();
        var entidades = Enum.GetNames<EntidadAuditoria>();

        // Verificar: coinciden en contenido y en orden, sin nombres sobrantes.
        Assert.Equal(accionesEsperadas, acciones);
        Assert.Equal(entidadesEsperadas, entidades);
    }
}
