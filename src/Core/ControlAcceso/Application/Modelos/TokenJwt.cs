namespace Core.ControlAcceso.Application.Modelos;

/// <summary>JWT de acceso emitido al iniciar sesión (RF-CA-03).</summary>
public sealed record TokenJwt(string Valor, DateTime ExpiraEn);