using System.Security.Cryptography;
using Core.ControlAcceso.Application.Interfaces;

namespace Core.ControlAcceso.Infrastructura.Servicios;

/// <summary>
/// Genera códigos de 32 bytes aleatorios (256 bits de entropía) codificados en
/// Base64URL sin relleno: no adivinables y seguros para usar en un enlace.
/// </summary>
public class TokenGeneradorCriptografico : ITokenGenerador
{
    public string Generar()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}