namespace Core.ControlAcceso.Application.Opciones;

/// <summary>
/// Configuración del flujo de recuperación de contraseña (sección "Recuperacion").
/// No contiene secretos (RD-10): solo la plantilla del enlace y la vigencia del código.
/// </summary>
public sealed class OpcionesRecuperacionContrasena
{
    public const string Seccion = "Recuperacion";

    /// <summary>
    /// Plantilla del enlace que recibe el usuario. El marcador <c>{codigo}</c> se
    /// reemplaza por el código generado (RF-CA-10, RF-CA-13).
    /// </summary>
    public string UrlRecuperacionPlantilla { get; set; } = string.Empty;

    /// <summary>Horas de validez del código de recuperación (RF-CA-11).</summary>
    public int HorasVencimientoCodigo { get; set; } = 1;
}
