namespace Core.ControlAcceso.Application.Modelos;

/// <summary>
/// DTO de respuesta para el listado de usuarios (RF-CA-21): expone solo datos no
/// sensibles. NUNCA incluye <c>ContraseñaHash</c> ni tokens.
/// </summary>
public sealed record UsuarioResumen(
    int Id,
    string Nombre,
    string Correo,
    string Rol,
    bool Activo);