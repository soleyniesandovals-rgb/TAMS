namespace Core.ControlAcceso.Application.Interfaces;

/// <summary>
/// Genera códigos de token criptográficamente aleatorios y no adivinables (RD-12).
/// </summary>
public interface ITokenGenerador
{
    string Generar();
}