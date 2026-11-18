namespace Core.ControlAcceso.Application.Modelos;

/// <summary>
/// Resultado de una pasada del procesador de correos (RF-NOT-10, RF-NOT-12).
/// </summary>
public sealed record ResultadoProcesamientoCorreos(int Enviados, int Fallidos);
