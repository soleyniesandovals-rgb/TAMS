using Core.ControlAcceso.Application.Interfaces;
using Core.ControlAcceso.Application.Opciones;
using Core.ControlAcceso.Infrastructura.Persistence;
using Tams.Negocio.Infrastructura.Persistence;

// Comando independiente de la API web: "dotnet run -- procesar-correos".
// Procesa los correos pendientes una sola vez y termina; NO se ejecuta en
// cada petición HTTP (RF-NOT-08).
if (args.Length > 0 && args[0].Equals("procesar-correos", StringComparison.OrdinalIgnoreCase))
{
    return await EjecutarComandoProcesarCorreosAsync(args);
}

var builder = WebApplication.CreateBuilder(args);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();

// RD-10: las cadenas de conexión se leen de variables de entorno; el repositorio
// nunca contiene conexiones reales ni claves.
var conexionCore = Entorno.Obtener("TAMS_CORE_CONNECTION_STRING");
var conexionNegocio = Entorno.Obtener("TAMS_NEGOCIO_CONNECTION_STRING");

// RD-03: cada módulo usa su propio DbContext y esquema ("core" y "negocio").
builder.Services.AddCoreDbContext(conexionCore);
builder.Services.AddCoreControlAcceso();
builder.Services.Configure<OpcionesActivacionCuenta>(builder.Configuration.GetSection(OpcionesActivacionCuenta.Seccion));

builder.Services.AddTamsNegocioDbContext(conexionNegocio);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();

return 0;

/// <summary>
/// Ejecuta el procesador de la cola de correos una sola vez y sale. Solo en este
/// comando se exigen las credenciales SMTP, leídas de variables de entorno
/// (RD-10, RF-NOT-13: TAMS_SMTP_HOST, TAMS_SMTP_PORT, TAMS_SMTP_USUARIO,
/// TAMS_SMTP_CONTRASENA, TAMS_SMTP_REMITENTE).
/// </summary>
static async Task<int> EjecutarComandoProcesarCorreosAsync(string[] args)
{
    try
    {
        var builder = WebApplication.CreateBuilder(args);

        var conexionCore = Entorno.Obtener("TAMS_CORE_CONNECTION_STRING");
        builder.Services.AddCoreDbContext(conexionCore);
        builder.Services.AddCoreControlAcceso();
        builder.Services.AddSmtpEnviadorCorreo();

        await using var app = builder.Build();

        var procesador = app.Services.GetRequiredService<IProcesadorCorreoEnCola>();
        var enviados = await procesador.ProcesarAsync();

        Console.WriteLine($"Procesador de correos terminado: {enviados} correo(s) enviado(s).");
        return 0;
    }
    catch (Exception ex)
    {
        // El correo fallido volvió a Pendiente para reintentarlo en la próxima
        // ejecución; la operación que lo originó ya terminó bien antes (RF-NOT-08).
        Console.Error.WriteLine($"Error al procesar correos: {ex.Message}");
        return 1;
    }
}

/// <summary>Helper de arranque: lee variables de entorno requeridas por RD-10.</summary>
internal static class Entorno
{
    public static string Obtener(string nombre)
        => Environment.GetEnvironmentVariable(nombre)
           ?? throw new InvalidOperationException(
               $"La variable de entorno {nombre} no está definida. "
               + "RD-10: la cadena de conexión debe venir de configuración/variables de entorno, no del repositorio.");
}