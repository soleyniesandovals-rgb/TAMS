using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Core.ControlAcceso.Application.Constantes;
using Core.ControlAcceso.Application.Interfaces;
using Core.ControlAcceso.Application.Modelos;
using Core.ControlAcceso.Domain.Entidades;
using Microsoft.IdentityModel.Tokens;

namespace Core.ControlAcceso.Infrastructura.Servicios;

public class JwtTokenGenerador(TimeProvider timeProvider, string secretoBase64) : ITokenJwtGenerador
{
    // RF-CA-03: vigencia sugerida del token de acceso.
    private static readonly TimeSpan Vigencia = TimeSpan.FromHours(1);

    public TokenJwt Generar(Usuario usuario)
    {
        var clave = new SymmetricSecurityKey(Convert.FromBase64String(secretoBase64));
        var credenciales = new SigningCredentials(clave, SecurityAlgorithms.HmacSha256);

        // RD-11: la hora proviene del TimeProvider inyectado (UTC); nunca DateTime.Now/UtcNow.
        var ahora = timeProvider.GetUtcNow();

        // RF-CA-03: el token lleva el Id, el correo, el nombre, el Rol y la
        // SesionVersion (claim personalizado) para validar la sesión (RF-CA-12, RF-CA-18).
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Correo),
            new Claim(ClaimTypes.Name, usuario.Nombre),
            new Claim(ClaimTypes.Role, usuario.Rol.ToString()),
            new Claim(ClaimsSesion.SesionVersion, usuario.SesionVersion.ToString(), ClaimValueTypes.Integer32),
        };

        var token = new JwtSecurityToken(
            claims: claims,
            notBefore: ahora.UtcDateTime,
            expires: ahora.UtcDateTime.Add(Vigencia),
            signingCredentials: credenciales);

        var valor = new JwtSecurityTokenHandler().WriteToken(token);
        return new TokenJwt(valor, token.ValidTo);
    }
}