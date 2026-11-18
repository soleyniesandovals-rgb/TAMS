using Core.ControlAcceso.Domain.Enums;

namespace Core.ControlAcceso.Application.Modelos;

/// <summary>
/// Datos públicos de un correo encolado para su consulta administrativa
/// (RF-NOT-11): nunca incluye el cuerpo, que puede contener enlaces o códigos.
/// </summary>
public sealed record CorreoEnColaResumen(
    int Id,
    string Destinatario,
    string Asunto,
    EstadoCorreo Estado,
    int Intentos,
    DateTime FechaCreacion,
    DateTime? FechaUltimoIntento,
    DateTime? FechaEnvio,
    string? UltimoError);
