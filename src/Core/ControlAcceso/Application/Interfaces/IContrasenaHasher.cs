namespace Core.ControlAcceso.Application.Interfaces;

/// <summary>
/// Abstracción del hash de contraseñas (RD-12). La implementación usa BCrypt (RF-CA-02)
/// y nunca se guarda la contraseña en texto plano.
/// </summary>
public interface IContrasenaHasher
{
    string Hash(string contrasena);

    bool Verificar(string contrasena, string hash);
}