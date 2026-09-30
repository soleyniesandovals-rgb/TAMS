namespace Core.ControlAcceso.Application.Opciones;

/// <summary>
/// Configuración del flujo de activación (sección "Cuentas" de appsettings).
/// No contiene secretos (RD-10): solo datos de enlace y validación del token.
/// </summary>
public sealed class OpcionesActivacionCuenta
{
    public const string Seccion = "Cuentas";

    /// <summary>
    /// Plantilla del enlace de activación. El marcador <c>{codigo}</c> se reemplaza
    /// por el token generado (RF-CA-15, RF-CA-17).
    /// </summary>
    public string UrlActivacionPlantilla { get; set; } = string.Empty;

    /// <summary>Horas de validez del token de activación (RF-CA-16).</summary>
    public int HorasVencimientoToken { get; set; } = 24;
}