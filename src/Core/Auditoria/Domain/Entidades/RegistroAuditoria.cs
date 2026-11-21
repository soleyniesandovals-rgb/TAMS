using Core.Auditoria.Domain.Enums;

namespace Core.Auditoria.Domain.Entidades;

/// <summary>
/// Registro del historial de auditoría (RF-AUD-01, RF-AUD-06). Es inmutable: solo se
/// crea con <see cref="Crear"/> y no expone setters públicos (CA-14).
/// </summary>
public class RegistroAuditoria
{
    private const int LongitudMaximaValor = 500;

    // EF Core necesita un constructor sin parámetros para materializar la entidad.
    private RegistroAuditoria()
    {
    }

    public int Id { get; private set; }

    /// <summary>Id de quien actuó.</summary>
    public int UsuarioId { get; private set; }

    public AccionAuditoria Accion { get; private set; }

    public EntidadAuditoria Entidad { get; private set; }

    public int EntidadId { get; private set; }

    /// <summary>Fecha y hora de la acción, siempre en UTC (RD-11).</summary>
    public DateTime FechaHora { get; private set; }

    public string? ValorAnterior { get; private set; }

    public string? ValorNuevo { get; private set; }

    /// <summary>
    /// Única forma de crear un registro (CA-14): valida identificadores, enums y la
    /// zona horaria, y recorta los valores legibles a 500 caracteres (CA-01, CA-11).
    /// </summary>
    public static RegistroAuditoria Crear(
        int usuarioId,
        AccionAuditoria accion,
        EntidadAuditoria entidad,
        int entidadId,
        DateTime fechaHora,
        string? valorAnterior,
        string? valorNuevo)
    {
        if (usuarioId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(usuarioId), "El usuario debe ser positivo.");
        }

        if (entidadId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(entidadId), "El id de la entidad debe ser positivo.");
        }

        if (!Enum.IsDefined(accion))
        {
            throw new ArgumentOutOfRangeException(nameof(accion), "La acción no es válida.");
        }

        if (!Enum.IsDefined(entidad))
        {
            throw new ArgumentOutOfRangeException(nameof(entidad), "La entidad no es válida.");
        }

        if (fechaHora.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("La fecha del registro debe estar en UTC.", nameof(fechaHora));
        }

        return new RegistroAuditoria
        {
            UsuarioId = usuarioId,
            Accion = accion,
            Entidad = entidad,
            EntidadId = entidadId,
            FechaHora = fechaHora,
            ValorAnterior = Recortar(valorAnterior),
            ValorNuevo = Recortar(valorNuevo),
        };
    }

    private static string? Recortar(string? valor)
        => valor is null || valor.Length <= LongitudMaximaValor
            ? valor
            : valor[..LongitudMaximaValor];
}
