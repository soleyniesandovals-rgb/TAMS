using Core.ControlAcceso.Domain.Excepciones;
using Microsoft.EntityFrameworkCore;
using Tams.Negocio.Application.Excepciones;
using Tams.Negocio.Application.Modelos;
using Tams.Negocio.Application.Servicios;
using Tams.Negocio.Domain.Entidades;
using Tams.Negocio.Infrastructura.Persistence;
using Xunit;

namespace Tests.Negocio;

/// <summary>
/// RF-NEG (RD-02, RD-07, RD-11): servicio de eventos del calendario con EF InMemory,
/// reloj y proveedor de feriados falsos. Ninguna prueba accede a la red.
/// </summary>
public class EventosCalendarioServicioTests
{
    private static readonly DateTimeOffset AhoraFalso = new(2026, 11, 27, 12, 30, 0, TimeSpan.Zero);
    private static readonly DateOnly Hoy = new(2026, 11, 27);
    private static readonly DateOnly Inicio = new(2026, 8, 1);
    private static readonly DateOnly Fin = new(2027, 6, 30);

    [Fact]
    public async Task RF_NEG_Crear_EventoValido_GuardaRecortadoYConFechaCreacionUtc()
    {
        // Preparar
        await using var contexto = CrearContexto();
        var anio = await SembrarAnioAsync(contexto);
        var proveedor = new ProveedorFeriadosFalso();
        var servicio = CrearServicio(contexto, proveedor);

        // Actuar
        var resumen = await servicio.CrearAsync(anio.Id, new DateOnly(2027, 3, 15), "  Entrega de notas  ", 7);

        // Verificar
        Assert.True(resumen.Id > 0);
        Assert.Equal(anio.Id, resumen.AnioEscolarId);
        Assert.Equal(new DateOnly(2027, 3, 15), resumen.Fecha);
        Assert.Equal("Entrega de notas", resumen.Descripcion);
        Assert.Equal(AhoraFalso.UtcDateTime, resumen.FechaCreacion);
        Assert.Equal(DateTimeKind.Utc, resumen.FechaCreacion.Kind);
        var guardado = Assert.Single(await contexto.EventosCalendario.AsNoTracking().ToListAsync());
        Assert.Equal(7, guardado.CreadoPorUsuarioId);
        Assert.Equal(AhoraFalso.UtcDateTime, guardado.FechaCreacion);
    }

    [Fact]
    public async Task RF_NEG_Crear_AnioInexistente_Lanza404YSinGuardar()
    {
        // Preparar
        await using var contexto = CrearContexto();
        var servicio = CrearServicio(contexto, new ProveedorFeriadosFalso());

        // Actuar
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(
            () => servicio.CrearAsync(99999, Hoy, "Evento", 7));

        // Verificar
        Assert.Equal(404, excepcion.CodigoHttp);
        Assert.Equal("El año escolar no existe.", excepcion.Message);
        Assert.Empty(await contexto.EventosCalendario.AsNoTracking().ToListAsync());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task RF_NEG_Crear_DescripcionVaciaOLarga_Lanza400YSinGuardar(string descripcion)
    {
        // Preparar
        await using var contexto = CrearContexto();
        var anio = await SembrarAnioAsync(contexto);
        var servicio = CrearServicio(contexto, new ProveedorFeriadosFalso());

        // Actuar
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(
            () => servicio.CrearAsync(anio.Id, Hoy, descripcion, 7));

        // Verificar
        Assert.Equal(400, excepcion.CodigoHttp);
        Assert.Equal(
            "La descripción del evento es obligatoria y no puede superar 200 caracteres.",
            excepcion.Message);
        Assert.Empty(await contexto.EventosCalendario.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task RF_NEG_Crear_DescripcionDe201Caracteres_Lanza400YSinGuardar()
    {
        // Preparar
        await using var contexto = CrearContexto();
        var anio = await SembrarAnioAsync(contexto);
        var servicio = CrearServicio(contexto, new ProveedorFeriadosFalso());

        // Actuar
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(
            () => servicio.CrearAsync(anio.Id, Hoy, new string('a', 201), 7));

        // Verificar
        Assert.Equal(400, excepcion.CodigoHttp);
        Assert.Equal(
            "La descripción del evento es obligatoria y no puede superar 200 caracteres.",
            excepcion.Message);
        Assert.Empty(await contexto.EventosCalendario.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task RF_NEG_Crear_FechaUnDiaFueraDelAnio_Lanza400YSinGuardar()
    {
        // Preparar
        await using var contexto = CrearContexto();
        var anio = await SembrarAnioAsync(contexto);
        var servicio = CrearServicio(contexto, new ProveedorFeriadosFalso());

        // Actuar
        var antes = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(
            () => servicio.CrearAsync(anio.Id, Inicio.AddDays(-1), "Evento", 7));
        var despues = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(
            () => servicio.CrearAsync(anio.Id, Fin.AddDays(1), "Evento", 7));

        // Verificar
        Assert.Equal(400, antes.CodigoHttp);
        Assert.Equal("La fecha del evento debe estar dentro del año escolar.", antes.Message);
        Assert.Equal(400, despues.CodigoHttp);
        Assert.Equal("La fecha del evento debe estar dentro del año escolar.", despues.Message);
        Assert.Empty(await contexto.EventosCalendario.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task RF_NEG_Listar_OrdenaPorFechaYIdYDevuelveFechaCreacionEnUtc()
    {
        // Preparar
        var nombreBase = $"EventosCalendario-{Guid.NewGuid():N}";
        await using var contexto = CrearContexto(nombreBase);
        var anio = await SembrarAnioAsync(contexto);
        var proveedor = new ProveedorFeriadosFalso();
        var servicio = CrearServicio(contexto, proveedor);
        await servicio.CrearAsync(anio.Id, new DateOnly(2027, 3, 15), "Tercero", 7);
        await servicio.CrearAsync(anio.Id, new DateOnly(2027, 1, 10), "Primero", 7);
        await servicio.CrearAsync(anio.Id, new DateOnly(2027, 3, 15), "Segundo", 7);

        // Actuar: se lee con otra conexión, como cuando la API consulta la base.
        await using var otraConexion = CrearContexto(nombreBase);
        var lista = await CrearServicio(otraConexion, proveedor).ListarAsync(anio.Id);

        // Verificar
        Assert.Equal(["Primero", "Tercero", "Segundo"], lista.Select(e => e.Descripcion));
        Assert.All(lista, e => Assert.Equal(DateTimeKind.Utc, e.FechaCreacion.Kind));
    }

    [Fact]
    public async Task RF_NEG_Listar_AnioInexistente_Lanza404()
    {
        // Preparar
        await using var contexto = CrearContexto();
        var servicio = CrearServicio(contexto, new ProveedorFeriadosFalso());

        // Actuar
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(
            () => servicio.ListarAsync(99999));

        // Verificar
        Assert.Equal(404, excepcion.CodigoHttp);
        Assert.Equal("El año escolar no existe.", excepcion.Message);
    }

    [Fact]
    public async Task RF_NEG_Borrar_EventoExistente_LoElimina()
    {
        // Preparar
        await using var contexto = CrearContexto();
        var anio = await SembrarAnioAsync(contexto);
        var servicio = CrearServicio(contexto, new ProveedorFeriadosFalso());
        var creado = await servicio.CrearAsync(anio.Id, Hoy, "Evento a borrar", 7);

        // Actuar
        await servicio.BorrarAsync(creado.Id);

        // Verificar
        Assert.Empty(await contexto.EventosCalendario.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task RF_NEG_Borrar_EventoInexistente_Lanza404()
    {
        // Preparar
        await using var contexto = CrearContexto();
        var servicio = CrearServicio(contexto, new ProveedorFeriadosFalso());

        // Actuar
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioExcepcion>(
            () => servicio.BorrarAsync(99999));

        // Verificar
        Assert.Equal(404, excepcion.CodigoHttp);
        Assert.Equal("El evento no existe.", excepcion.Message);
    }

    [Fact]
    public async Task RF_NEG_Proximos_IncluyeHoyYFechaFinYExcluyeFueraDelRango()
    {
        // Preparar
        await using var contexto = CrearContexto();
        var anio = await SembrarAnioAsync(contexto);
        var servicio = CrearServicio(contexto, new ProveedorFeriadosFalso());
        await servicio.CrearAsync(anio.Id, Hoy.AddDays(-1), "Ayer", 7);
        await servicio.CrearAsync(anio.Id, Hoy, "Hoy", 7);
        await servicio.CrearAsync(anio.Id, Fin, "El último día", 7);
        // Un evento posterior al fin no se puede crear; se siembra para comprobar el filtro.
        contexto.EventosCalendario.Add(new EventoCalendario
        {
            AnioEscolarId = anio.Id,
            Fecha = Fin.AddDays(1),
            Descripcion = "Después del fin",
            CreadoPorUsuarioId = 7,
        });
        await contexto.SaveChangesAsync();

        // Actuar
        var proximos = await servicio.ProximosAsync();

        // Verificar
        Assert.Equal(anio.Id, proximos.AnioEscolarId);
        Assert.True(proximos.FeriadosDisponibles);
        Assert.Equal(["Hoy", "El último día"], proximos.Elementos.Select(e => e.Descripcion));
        Assert.All(proximos.Elementos, e => Assert.Equal("Evento", e.Tipo));
    }

    [Fact]
    public async Task RF_NEG_Proximos_FeriadoYEventoElMismoDia_ElFeriadoVaPrimero()
    {
        // Preparar
        await using var contexto = CrearContexto();
        var anio = await SembrarAnioAsync(contexto);
        var proveedor = new ProveedorFeriadosFalso(_ =>
        [
            (new DateOnly(2026, 12, 25), "Navidad"),
            (Fin.AddDays(1), "Feriado fuera de rango"),
        ]);
        var servicio = CrearServicio(contexto, proveedor);
        var creado = await servicio.CrearAsync(anio.Id, new DateOnly(2026, 12, 25), "Evento escolar", 7);

        // Actuar
        var proximos = await servicio.ProximosAsync();

        // Verificar
        Assert.Equal(2, proximos.Elementos.Count);
        var feriado = proximos.Elementos[0];
        Assert.Equal(new DateOnly(2026, 12, 25), feriado.Fecha);
        Assert.Equal("Navidad", feriado.Descripcion);
        Assert.Equal("Feriado", feriado.Tipo);
        Assert.Null(feriado.EventoId);
        var evento = proximos.Elementos[1];
        Assert.Equal("Evento escolar", evento.Descripcion);
        Assert.Equal("Evento", evento.Tipo);
        Assert.Equal(creado.Id, evento.EventoId);
    }

    [Fact]
    public async Task RF_NEG_Proximos_FeriadoRepetidoMismaFechaYNombre_CuentaUnaVez()
    {
        // Preparar
        await using var contexto = CrearContexto();
        await SembrarAnioAsync(contexto);
        var fecha = new DateOnly(2027, 1, 6);
        var proveedor = new ProveedorFeriadosFalso(_ =>
        [
            (fecha, "Reyes Magos"),
            (fecha, "Reyes Magos"),
            (fecha, "Otro feriado"),
        ]);
        var servicio = CrearServicio(contexto, proveedor);

        // Actuar
        var proximos = await servicio.ProximosAsync();

        // Verificar
        Assert.Equal(2, proximos.Elementos.Count);
        Assert.Single(proximos.Elementos, e => e.Descripcion == "Reyes Magos");
        Assert.Single(proximos.Elementos, e => e.Descripcion == "Otro feriado");
    }

    [Fact]
    public async Task RF_NEG_Proximos_RangoQueCruzaDosAnios_PideUnFeriadoPorAnio()
    {
        // Preparar
        await using var contexto = CrearContexto();
        await SembrarAnioAsync(contexto);
        var proveedor = new ProveedorFeriadosFalso(_ => []);
        var servicio = CrearServicio(contexto, proveedor);

        // Actuar
        await servicio.ProximosAsync();

        // Verificar
        Assert.Equal([2026, 2027], proveedor.AniosSolicitados);
    }

    [Fact]
    public async Task RF_NEG_Proximos_SinAnioActivo_DevuelveVacioSinLlamarAlProveedor()
    {
        // Preparar
        await using var contexto = CrearContexto();
        await SembrarAnioAsync(contexto, activo: false);
        var proveedor = new ProveedorFeriadosFalso();
        var servicio = CrearServicio(contexto, proveedor);

        // Actuar
        var proximos = await servicio.ProximosAsync();

        // Verificar
        Assert.Null(proximos.AnioEscolarId);
        Assert.False(proximos.FeriadosDisponibles);
        Assert.Empty(proximos.Elementos);
        Assert.Empty(proveedor.AniosSolicitados);
    }

    [Fact]
    public async Task RF_NEG_Proximos_ProveedorNoConfigurado_DevuelveSoloEventos()
    {
        // Preparar
        await using var contexto = CrearContexto();
        var anio = await SembrarAnioAsync(contexto);
        var proveedor = new ProveedorFeriadosFalso(
            excepcion: new ServicioFeriadosNoConfiguradoExcepcion());
        var servicio = CrearServicio(contexto, proveedor);
        await servicio.CrearAsync(anio.Id, Hoy, "Evento sin feriados", 7);

        // Actuar
        var proximos = await servicio.ProximosAsync();

        // Verificar
        Assert.Equal(anio.Id, proximos.AnioEscolarId);
        Assert.False(proximos.FeriadosDisponibles);
        var evento = Assert.Single(proximos.Elementos);
        Assert.Equal("Evento sin feriados", evento.Descripcion);
        Assert.Equal("Evento", evento.Tipo);
    }

    [Fact]
    public async Task RF_NEG_Proximos_ProveedorNoDisponible_DevuelveSoloEventos()
    {
        // Preparar
        await using var contexto = CrearContexto();
        var anio = await SembrarAnioAsync(contexto);
        var proveedor = new ProveedorFeriadosFalso(
            excepcion: new ServicioFeriadosNoDisponibleExcepcion());
        var servicio = CrearServicio(contexto, proveedor);
        await servicio.CrearAsync(anio.Id, Hoy, "Evento sin feriados", 7);

        // Actuar
        var proximos = await servicio.ProximosAsync();

        // Verificar
        Assert.Equal(anio.Id, proximos.AnioEscolarId);
        Assert.False(proximos.FeriadosDisponibles);
        var evento = Assert.Single(proximos.Elementos);
        Assert.Equal("Evento sin feriados", evento.Descripcion);
        Assert.Equal("Evento", evento.Tipo);
    }

    private static EventosCalendarioServicio CrearServicio(
        TamsDbContext contexto,
        IProveedorFeriados proveedor)
        => new(contexto, proveedor, new TimeProviderFijo(AhoraFalso));

    private static TamsDbContext CrearContexto(string? nombreBase = null)
    {
        var opciones = new DbContextOptionsBuilder<TamsDbContext>()
            .UseInMemoryDatabase(nombreBase ?? $"EventosCalendario-{Guid.NewGuid():N}")
            .Options;

        return new TamsDbContext(opciones, new TimeProviderFijo(AhoraFalso));
    }

    private static async Task<AnioEscolar> SembrarAnioAsync(
        TamsDbContext contexto,
        bool activo = true,
        DateOnly? fechaInicio = null,
        DateOnly? fechaFin = null)
    {
        var anio = new AnioEscolar
        {
            Nombre = $"Año-{Guid.NewGuid():N}",
            FechaInicio = fechaInicio ?? Inicio,
            FechaFin = fechaFin ?? Fin,
            EsActivo = activo,
        };

        contexto.AniosEscolares.Add(anio);
        await contexto.SaveChangesAsync();

        return anio;
    }

    private sealed class TimeProviderFijo(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }

    private sealed class ProveedorFeriadosFalso : IProveedorFeriados
    {
        private readonly Func<int, IReadOnlyList<(DateOnly Fecha, string Nombre)>> _feriados;
        private readonly Exception? _excepcion;

        public ProveedorFeriadosFalso(
            Func<int, IReadOnlyList<(DateOnly Fecha, string Nombre)>>? feriados = null,
            Exception? excepcion = null)
        {
            _feriados = feriados ?? (_ => []);
            _excepcion = excepcion;
        }

        public List<int> AniosSolicitados { get; } = [];

        public Task<IReadOnlyList<(DateOnly Fecha, string Nombre)>> ObtenerFeriadosNacionalesAsync(
            int anio,
            CancellationToken cancellationToken = default)
        {
            AniosSolicitados.Add(anio);

            if (_excepcion is not null)
            {
                throw _excepcion;
            }

            return Task.FromResult(_feriados(anio));
        }
    }
}
