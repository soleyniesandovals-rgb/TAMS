namespace Core.ControlAcceso.Infrastructura.Servicios;

/// <summary>
/// Credenciales del servidor SMTP (RD-10, RF-NOT-13): se leen ÚNICAMENTE de variables
/// de entorno; nunca de appsettings.json ni del repositorio.
/// </summary>
public sealed class OpcionesSmtp
{
    public string Host { get; }

    public int Puerto { get; }

    public string Usuario { get; }

    public string Contrasena { get; }

    /// <summary>Dirección "De" que verá el destinatario del correo.</summary>
    public string Remitente { get; }

    public OpcionesSmtp(string host, int puerto, string usuario, string contrasena, string remitente)
    {
        Host = host;
        Puerto = puerto;
        Usuario = usuario;
        Contrasena = contrasena;
        Remitente = remitente;
    }

    /// <summary>
    /// Variables de entorno utilizadas (nombres exactos):
    /// TAMS_SMTP_HOST, TAMS_SMTP_PORT, TAMS_SMTP_USUARIO,
    /// TAMS_SMTP_CONTRASENA, TAMS_SMTP_REMITENTE.
    /// </summary>
    public static OpcionesSmtp DesdeVariablesEntorno()
    {
        string Obtener(string nombre)
            => Environment.GetEnvironmentVariable(nombre)
               ?? throw new InvalidOperationException(
                   $"La variable de entorno {nombre} no está definida (RD-10, RF-NOT-13).");

        var host = Obtener("TAMS_SMTP_HOST");

        var puertoTexto = Obtener("TAMS_SMTP_PORT");
        if (!int.TryParse(puertoTexto, out var puerto) || puerto is < 1 or > 65535)
        {
            throw new InvalidOperationException(
                "La variable de entorno TAMS_SMTP_PORT debe ser un puerto válido (1-65535) (RF-NOT-13).");
        }

        var usuario = Obtener("TAMS_SMTP_USUARIO");
        var contrasena = Obtener("TAMS_SMTP_CONTRASENA");
        var remitente = Obtener("TAMS_SMTP_REMITENTE");

        return new OpcionesSmtp(host, puerto, usuario, contrasena, remitente);
    }
}