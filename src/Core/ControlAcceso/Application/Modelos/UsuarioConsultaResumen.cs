namespace Core.ControlAcceso.Application.Modelos;

/// <summary>
/// Datos públicos y de solo lectura de un usuario para otros módulos (RF-NEG-02,
/// RD-03): nunca incluye la contraseña, su hash, tokens ni datos de sesión.
/// </summary>
public sealed record UsuarioConsultaResumen(
    int Id,
    string Nombre,
    string Correo,
    string Rol,
    bool Activo);
