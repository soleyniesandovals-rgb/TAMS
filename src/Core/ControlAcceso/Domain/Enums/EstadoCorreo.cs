namespace Core.ControlAcceso.Domain.Enums;

/// <summary>Estado de un correo dentro de la cola de correos del Core.</summary>
public enum EstadoCorreo
{
    Pendiente,

    /// <summary>Reclamado por una ejecución del procesador; envío en curso (RF-NOT-09).</summary>
    Enviando,

    Enviado,

    /// <summary>Se agotaron los intentos de envío; no se vuelve a intentar (RF-NOT-10).</summary>
    Fallido,
}