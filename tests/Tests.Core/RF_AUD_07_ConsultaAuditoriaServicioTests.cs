using System.Reflection;
using System.Text.Json;
using Core.Auditoria.Application.Interfaces;
using Core.Auditoria.Application.Servicios;
using Core.Auditoria.Domain.Entidades;
using Core.Auditoria.Domain.Enums;
using Core.ControlAcceso.Domain.Excepciones;
using Core.ControlAcceso.Infrastructura.Persistence;
using Core.ControlAcceso.Infrastructura.Repositorios;
using Microsoft.EntityFrameworkCore;

namespace Tests.Core;

/// <summary>
/// RF-AUD-07 (CA-15 a CA-18, RD-11, RD-07): validación de los filtros recibidos como
/// texto, combinación de filtros, extremos inclusivos y fecha UTC en la respuesta.
/// Sin levantar la aplicación ni usar SQL Server (RD-12).
/// </summary>
public class ConsultaAuditoriaServicioTests
{
    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("abc")]
    public async Task RF_AUD_07_UsuarioIdInvalido_Devuelve400ConMensajeClaro(string usuarioId)
    {
        // Preparar: servicio sobre una base vacía. CA-18 exige rechazar entradas no positivas.
        await using var contexto = CrearContexto();
        var servicio = CrearServicio(contexto);

        // Actuar: se consulta con un usuarioId que no es un entero positivo.
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(
            () => servicio.ListarAsync(usuarioId, null, null, null, CancellationToken.None));

        // Verificar: 400 controlado con un mensaje que nombra el filtro.
        Assert.Equal(400, excepcion.CodigoHttp);
        Assert.Equal("usuarioId debe ser un entero positivo.", excepcion.Message);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("1")]
    [InlineData("Usuario,Documento")]
    [InlineData("EntidadInexistente")]
    public async Task RF_AUD_07_EntidadQueNoEsUnNombreExacto_Devuelve400ConMensajeClaro(string entidad)
    {
        // Preparar: la lección A3-H2 prohíbe números y combinaciones con comas (CA-18).
        await using var contexto = CrearContexto();
        var servicio = CrearServicio(contexto);

        // Actuar: se consulta con una entidad inválida.
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(
            () => servicio.ListarAsync(null, entidad, null, null, CancellationToken.None));

        // Verificar: 400 con los valores permitidos, sin aceptar la entrada.
        Assert.Equal(400, excepcion.CodigoHttp);
        Assert.Equal(
            "La entidad indicada no es válida. Valores permitidos: SolicitudPermiso, Documento, Usuario.",
            excepcion.Message);
    }

    [Theory]
    [InlineData("desde", "2026/11/21")]
    [InlineData("hasta", "21-11-2026")]
    [InlineData("desde", "2026-2-3")]
    public async Task RF_AUD_07_FechaConFormatoDistintoAAAA_MM_DD_Devuelve400ConMensajeClaro(
        string campo,
        string valor)
    {
        // Preparar: cultura invariante y formato estricto AAAA-MM-DD (CA-18).
        await using var contexto = CrearContexto();
        var servicio = CrearServicio(contexto);

        // Actuar: se consulta con el extremo inválido en el campo indicado.
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(() => campo == "desde"
            ? servicio.ListarAsync(null, null, valor, null, CancellationToken.None)
            : servicio.ListarAsync(null, null, null, valor, CancellationToken.None));

        // Verificar: 400 y el mensaje distingue qué extremo falló.
        Assert.Equal(400, excepcion.CodigoHttp);
        Assert.Equal($"{campo} debe tener el formato AAAA-MM-DD.", excepcion.Message);
    }

    [Fact]
    public async Task RF_AUD_07_DesdePosteriorAHasta_Devuelve400ConMensajeClaro()
    {
        // Preparar: un rango invertido con ambos extremos bien formateados (CA-18).
        await using var contexto = CrearContexto();
        var servicio = CrearServicio(contexto);

        // Actuar: se consulta con desde posterior a hasta.
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(
            () => servicio.ListarAsync(null, null, "2026-11-22", "2026-11-21", CancellationToken.None));

        // Verificar: 400 controlado en lugar de una lista vacía engañosa.
        Assert.Equal(400, excepcion.CodigoHttp);
        Assert.Equal("desde no puede ser posterior a hasta.", excepcion.Message);
    }

    [Theory]
    [InlineData("0001-01-01")]
    [InlineData("9999-12-31")]
    public async Task RF_AUD_07_FechaExtremaValida_SinExcepcionYListaVacia(string fecha)
    {
        // Preparar: una base vacía; la fecha extrema tiene formato válido (A3-H1, CA-18).
        await using var contexto = CrearContexto();
        var servicio = CrearServicio(contexto);

        // Actuar: se consulta usando ese extremo como desde y como hasta.
        var registros = await servicio.ListarAsync(null, null, fecha, fecha, CancellationToken.None);

        // Verificar: 200 con lista vacía; nunca 500 por desbordar al calcular el día siguiente.
        Assert.Empty(registros);
    }

    [Theory]
    [InlineData("0001-01-01", 1, 1, 1)]
    [InlineData("9999-12-31", 9999, 12, 31)]
    public async Task RF_AUD_07_HastaFechaExtrema_RegistroDelMismoDiaSigueIncluido(
        string fecha,
        int anio,
        int mes,
        int dia)
    {
        // Preparar: un registro a las 23:59 del día extremo (A3-H1, CA-16, CA-18).
        await using var contexto = CrearContexto();
        await RegistrarAsync(contexto, 3, AccionAuditoria.SubirDocumento, EntidadAuditoria.Documento, 7,
            new DateTime(anio, mes, dia, 23, 59, 0, DateTimeKind.Utc));
        var servicio = CrearServicio(contexto);

        // Actuar: el rango termina en ese mismo día representable.
        var registros = await servicio.ListarAsync(null, null, fecha, fecha, CancellationToken.None);

        // Verificar: el extremo inclusivo sigue contando cualquier hora del último día.
        Assert.Single(registros);
    }

    [Fact]
    public async Task RF_AUD_07_CadaFiltroPorSeparadoYCombinados_ReducenElResultado()
    {
        // Preparar: cuatro registros que difieren en usuario, entidad y fecha.
        await using var contexto = CrearContexto();
        await SembrarFiltrosAsync(contexto);
        var servicio = CrearServicio(contexto);

        // Actuar: se aplica cada filtro solo y después los cuatro juntos.
        var porUsuario = await servicio.ListarAsync("3", null, null, null, CancellationToken.None);
        var porEntidad = await servicio.ListarAsync(null, "documento", null, null, CancellationToken.None);
        var porDesde = await servicio.ListarAsync(null, null, "2026-11-21", null, CancellationToken.None);
        var porHasta = await servicio.ListarAsync(null, null, null, "2026-11-20", CancellationToken.None);
        var combinados = await servicio.ListarAsync(
            "3", "Documento", "2026-11-21", "2026-11-21", CancellationToken.None);

        // Verificar: CA-16 cada filtro reduce y se pueden combinar; la entidad no distingue mayúsculas.
        Assert.Equal([2, 3, 1], porUsuario.Select(r => r.Id).ToList());
        Assert.Equal([4, 2, 1], porEntidad.Select(r => r.Id).ToList());
        Assert.Equal([4, 2, 3], porDesde.Select(r => r.Id).ToList());
        Assert.Equal([1], porHasta.Select(r => r.Id).ToList());
        Assert.Equal([2], combinados.Select(r => r.Id).ToList());
    }

    [Fact]
    public async Task RF_AUD_07_HastaUltimoDia_Incluye2359YExcluyeElDiaSiguiente()
    {
        // Preparar: un registro a las 23:59 del último día y otro a medianoche del siguiente.
        await using var contexto = CrearContexto();
        await RegistrarAsync(contexto, 3, AccionAuditoria.SubirDocumento, EntidadAuditoria.Documento, 1,
            new DateTime(2026, 11, 21, 23, 59, 0, DateTimeKind.Utc));
        await RegistrarAsync(contexto, 3, AccionAuditoria.SubirDocumento, EntidadAuditoria.Documento, 2,
            new DateTime(2026, 11, 22, 0, 0, 0, DateTimeKind.Utc));
        var servicio = CrearServicio(contexto);

        // Actuar: se consulta con hasta = 2026-11-21.
        var registros = await servicio.ListarAsync(null, null, null, "2026-11-21", CancellationToken.None);

        // Verificar: CA-16 el día de hasta cuenta completo y el día siguiente no.
        var registro = Assert.Single(registros);
        Assert.Equal(1, registro.Id);
    }

    [Fact]
    public async Task RF_AUD_07_RangosDistintos_DevuelvenResultadosDistintos()
    {
        // Preparar: registros en dos días distintos sobre la misma base.
        await using var contexto = CrearContexto();
        await SembrarFiltrosAsync(contexto);
        var servicio = CrearServicio(contexto);

        // Actuar: se consultan dos rangos que no coinciden.
        var primerRango = await servicio.ListarAsync(null, null, "2026-11-20", "2026-11-20", CancellationToken.None);
        var segundoRango = await servicio.ListarAsync(null, null, "2026-11-21", "2026-11-21", CancellationToken.None);

        // Verificar: CA-16 cada rango reduce a lo que cumple y no son el mismo resultado.
        Assert.Equal([1], primerRango.Select(r => r.Id).ToList());
        Assert.Equal([4, 2, 3], segundoRango.Select(r => r.Id).ToList());
        Assert.NotEqual(primerRango.Select(r => r.Id), segundoRango.Select(r => r.Id));
    }

    [Fact]
    public async Task RF_AUD_07_FiltroSinCoincidencias_DevuelveListaVacia()
    {
        // Preparar: una base con registros y un servicio de consulta.
        await using var contexto = CrearContexto();
        await SembrarFiltrosAsync(contexto);
        var servicio = CrearServicio(contexto);

        // Actuar: se filtra por un usuario que no tiene registros.
        var registros = await servicio.ListarAsync("999", null, null, null, CancellationToken.None);

        // Verificar: CA-16 respuesta 200 con lista vacía, sin lanzar excepciones.
        Assert.Empty(registros);
    }

    [Fact]
    public async Task RF_AUD_07_SinFiltros_DevuelveTodosLosCamposOrdenadosDeMasRecienteAMasAntiguo()
    {
        // Preparar: registros con valores anterior y nuevo distintos, y un desempate por Id.
        await using var contexto = CrearContexto();
        await RegistrarAsync(contexto, 7, AccionAuditoria.SubirDocumento, EntidadAuditoria.Documento, 101,
            new DateTime(2026, 11, 20, 8, 0, 0, DateTimeKind.Utc), null, "a.pdf (pdf, 10 bytes)");
        await RegistrarAsync(contexto, 3, AccionAuditoria.AprobarSolicitud, EntidadAuditoria.SolicitudPermiso, 55,
            new DateTime(2026, 11, 21, 23, 59, 0, DateTimeKind.Utc), "Pendiente", "Aplicada (Vacaciones)");
        await RegistrarAsync(contexto, 3, AccionAuditoria.CambiarRol, EntidadAuditoria.Usuario, 9,
            new DateTime(2026, 11, 21, 23, 59, 0, DateTimeKind.Utc), "Estándar", "Administrador");
        var servicio = CrearServicio(contexto);

        // Actuar: se consulta sin filtros.
        var registros = await servicio.ListarAsync(null, null, null, null, CancellationToken.None);

        // Verificar: CA-15 todos los campos, FechaHora y luego Id descendentes.
        Assert.Equal([3, 2, 1], registros.Select(r => r.Id).ToList());
        var reciente = registros[0];
        Assert.Equal(3, reciente.UsuarioId);
        Assert.Equal(AccionAuditoria.CambiarRol, reciente.Accion);
        Assert.Equal(EntidadAuditoria.Usuario, reciente.Entidad);
        Assert.Equal(9, reciente.EntidadId);
        Assert.Equal(new DateTime(2026, 11, 21, 23, 59, 0, DateTimeKind.Utc), reciente.FechaHora);
        Assert.Equal("Estándar", reciente.ValorAnterior);
        Assert.Equal("Administrador", reciente.ValorNuevo);
    }

    [Fact]
    public async Task RD_11_FechaHoraDeLaRespuesta_TieneMarcaUtcYSerializaConZ()
    {
        // Preparar: un registro leído desde la base y un servicio de consulta.
        await using var contexto = CrearContexto();
        await RegistrarAsync(contexto, 4, AccionAuditoria.DesactivarUsuario, EntidadAuditoria.Usuario, 12,
            new DateTime(2026, 11, 21, 23, 59, 0, DateTimeKind.Utc));
        var servicio = CrearServicio(contexto);

        // Actuar: se consulta y se serializa la respuesta con las opciones de la API.
        var registros = await servicio.ListarAsync(null, null, null, null, CancellationToken.None);
        var json = JsonSerializer.Serialize(registros, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        // Verificar: RD-11 la fecha se marca UTC y se escribe terminando en Z.
        Assert.Equal(DateTimeKind.Utc, Assert.Single(registros).FechaHora.Kind);
        Assert.Contains("\"fechaHora\":\"2026-11-21T23:59:00Z\"", json, StringComparison.Ordinal);
    }

    [Fact]
    public void RF_AUD_06_ServicioDeConsulta_SoloListaSinEditarNiBorrar()
    {
        // Preparar: la interfaz, su implementación y los verbos prohibidos (CA-14).
        var verbosProhibidos = new[]
        {
            "Actualizar", "Editar", "Modificar", "Borrar", "Eliminar", "Update", "Delete", "Remove",
        };

        // Actuar: se listan los métodos públicos declarados en ambos tipos.
        var metodos = typeof(IConsultaAuditoriaServicio).GetMethods()
            .Concat(typeof(ConsultaAuditoriaServicio).GetMethods(
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            .Select(m => m.Name)
            .Distinct()
            .ToList();

        // Verificar: CA-14 no hay operaciones de escritura ni borrado en la consulta.
        Assert.Equal(new[] { "ListarAsync" }, metodos);
        Assert.DoesNotContain(metodos, nombre =>
            verbosProhibidos.Any(verbo => nombre.Contains(verbo, StringComparison.OrdinalIgnoreCase)));
    }

    private static ConsultaAuditoriaServicio CrearServicio(CoreDbContext contexto)
        => new(new CoreUnidadDeTrabajo(contexto));

    private static CoreDbContext CrearContexto()
    {
        var opciones = new DbContextOptionsBuilder<CoreDbContext>()
            .UseInMemoryDatabase($"auditoria-consulta-servicio-{Guid.NewGuid():N}")
            .Options;
        return new CoreDbContext(opciones, TimeProvider.System);
    }

    private static async Task SembrarFiltrosAsync(CoreDbContext contexto)
    {
        await RegistrarAsync(contexto, 3, AccionAuditoria.SubirDocumento, EntidadAuditoria.Documento, 1,
            new DateTime(2026, 11, 20, 10, 0, 0, DateTimeKind.Utc), null, "Antes del rango");
        await RegistrarAsync(contexto, 3, AccionAuditoria.SubirDocumento, EntidadAuditoria.Documento, 2,
            new DateTime(2026, 11, 21, 23, 59, 0, DateTimeKind.Utc), null, "Dentro del rango");
        await RegistrarAsync(contexto, 3, AccionAuditoria.CambiarRol, EntidadAuditoria.Usuario, 3,
            new DateTime(2026, 11, 21, 12, 0, 0, DateTimeKind.Utc), "Estándar", "Administrador");
        await RegistrarAsync(contexto, 8, AccionAuditoria.SubirDocumento, EntidadAuditoria.Documento, 4,
            new DateTime(2026, 11, 21, 23, 59, 0, DateTimeKind.Utc), null, "Otro usuario");
    }

    private static async Task RegistrarAsync(
        CoreDbContext contexto,
        int usuarioId,
        AccionAuditoria accion,
        EntidadAuditoria entidad,
        int entidadId,
        DateTime fechaHora,
        string? valorAnterior = null,
        string? valorNuevo = null)
    {
        contexto.RegistrosAuditoria.Add(RegistroAuditoria.Crear(
            usuarioId, accion, entidad, entidadId, fechaHora, valorAnterior, valorNuevo));
        await contexto.SaveChangesAsync();
    }
}
