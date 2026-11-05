using Core.ControlAcceso.Domain.Entidades;

namespace Core.Documentos.Domain.Entidades;

/// <summary>
/// Documento subido por un usuario (RF-DOC-01). El archivo físico vive fuera de la
/// base de datos; aquí solo se guarda su ruta relativa de almacenamiento, nunca la
/// ruta física absoluta del servidor.
/// </summary>
public class Documento
{
    public int Id { get; set; }

    /// <summary>Nombre original del archivo, ya limpio con <see cref="Path.GetFileName"/>.</summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Tipo MIME informado por el cliente (por ejemplo, "application/pdf").</summary>
    public string TipoContenido { get; set; } = string.Empty;

    /// <summary>Tamaño del archivo en bytes.</summary>
    public long TamanoBytes { get; set; }

    /// <summary>Momento (UTC) en que se subió el documento (RD-11).</summary>
    public DateTime FechaSubida { get; set; }

    /// <summary>FK al <see cref="Usuario"/> dueño del documento.</summary>
    public int UsuarioId { get; set; }

    /// <summary>Nombre físico relativo del archivo dentro de la carpeta de almacenamiento.</summary>
    public string RutaAlmacenamiento { get; set; } = string.Empty;

    /// <summary>Marca de borrado lógico; el archivo físico no se elimina (RF-DOC-04).</summary>
    public bool Eliminado { get; set; }

    public Usuario Usuario { get; set; } = null!;
}
