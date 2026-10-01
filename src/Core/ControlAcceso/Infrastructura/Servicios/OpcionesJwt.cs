namespace Core.ControlAcceso.Infrastructura.Servicios;

/// <summary>
/// Clave de firma de los JWT (RD-10, RF-CA-03): se lee ÚNICAMENTE de variables de
/// entorno, nunca de appsettings.json ni del repositorio.
/// </summary>
public static class OpcionesJwt
{
    /// <summary>
    /// Variable de entorno exacta con el secreto de firma: un valor Base64 de al menos
    /// 32 bytes decodificados (256 bits), por ejemplo:
    /// <c>$env:TAMS_JWT_SECRETO="&lt;Base64&gt;"</c>
    /// </summary>
    public const string VariableSecreto = "TAMS_JWT_SECRETO";

    /// <summary>Lee y valida el secreto desde la variable de entorno (RD-10).</summary>
    public static string SecretoDesdeVariableEntorno()
    {
        var secreto = Environment.GetEnvironmentVariable(VariableSecreto);
        if (string.IsNullOrWhiteSpace(secreto))
        {
            throw new InvalidOperationException(
                $"La variable de entorno {VariableSecreto} no está definida (RD-10). "
                + "Debe ser un valor Base64 de al menos 32 bytes decodificados.");
        }

        byte[] bytes;
        try
        {
            bytes = Convert.FromBase64String(secreto);
        }
        catch (FormatException)
        {
            throw new InvalidOperationException(
                $"La variable de entorno {VariableSecreto} debe ser un valor Base64 válido (RD-10).");
        }

        if (bytes.Length < 32)
        {
            throw new InvalidOperationException(
                $"La variable de entorno {VariableSecreto} debe tener al menos 32 bytes decodificados (RD-10).");
        }

        return secreto;
    }
}