using Core.ControlAcceso.Application.Interfaces;

namespace Core.ControlAcceso.Infrastructura.Servicios;

/// <summary>Hash de contraseñas con BCrypt (RF-CA-02); nunca se almacena en texto plano.</summary>
public class BcryptContrasenaHasher : IContrasenaHasher
{
    // BCrypt.Net-Next 4.2.1 en net10.0: el identificador "BCrypt" queda sombreado por un
    // espacio de nombres homónimo del grafo, por lo que se usa el nombre calificado.
    public string Hash(string contrasena) => BCrypt.Net.BCrypt.HashPassword(contrasena);

    public bool Verificar(string contrasena, string hash) => BCrypt.Net.BCrypt.Verify(contrasena, hash);
}