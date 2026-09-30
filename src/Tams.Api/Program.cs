using Core.ControlAcceso.Application.Opciones;
using Core.ControlAcceso.Infrastructura.Persistence;
using Tams.Negocio.Infrastructura.Persistence;

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

/// <summary>Helper de arranque: lee variables de entorno requeridas por RD-10.</summary>
internal static class Entorno
{
    public static string Obtener(string nombre)
        => Environment.GetEnvironmentVariable(nombre)
           ?? throw new InvalidOperationException(
               $"La variable de entorno {nombre} no está definida. "
               + "RD-10: la cadena de conexión debe venir de configuración/variables de entorno, no del repositorio.");
}