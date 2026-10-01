using Core.ControlAcceso.Application.Modelos;
using Core.ControlAcceso.Domain.Entidades;

namespace Core.ControlAcceso.Application.Interfaces;

public interface ITokenJwtGenerador
{
    /// <summary>Emite un JWT de acceso con los claims de identidad y sesión (RF-CA-03).</summary>
    TokenJwt Generar(Usuario usuario);
}