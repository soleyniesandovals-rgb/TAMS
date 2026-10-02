using Core.ControlAcceso.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace Tams.Api.Seguridad;

/// <summary>
/// Exige que el usuario autenticado tenga el rol indicado (RF-CA-05, RF-CA-06, RD-06).
/// <para>
/// Hereda de <see cref="AuthorizeAttribute"/> para reutilizar el motor de autorización
/// de ASP.NET Core: al fijar <see cref="AuthorizeAttribute.Roles"/> se construye una
/// política que exige autenticación y el rol. Así la exigencia se declara en UN SOLO
/// lugar por operación, con el rol fuertemente tipado.
/// </para>
/// <para>
/// El rechazo ocurre siempre del lado del servidor: sin token responde 401 y con un
/// token válido de otro rol responde 403, aunque la petición se construya a mano
/// (Invoke-RestMethod, curl, Postman...), sin pasar por ninguna UI (RD-06).
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class RequiereRolAttribute : AuthorizeAttribute
{
    public RequiereRolAttribute(RolUsuario rol) => Roles = rol.ToString();
}