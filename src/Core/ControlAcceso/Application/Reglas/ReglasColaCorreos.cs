namespace Core.ControlAcceso.Application.Reglas;

/// <summary>
/// Reglas de la cola de correos del Core (RF-NOT-10): centraliza el máximo de
/// intentos de envío para que el procesador y las pruebas lo compartan.
/// </summary>
public static class ReglasColaCorreos
{
    public const int MaximoIntentos = 3;
}
