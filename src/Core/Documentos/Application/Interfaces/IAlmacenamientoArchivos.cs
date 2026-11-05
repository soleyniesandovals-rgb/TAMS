namespace Core.Documentos.Application.Interfaces;

/// <summary>
/// Abstracción del almacenamiento físico de archivos (RF-DOC-01). La aplicación
/// genera el nombre físico (un Guid) y nunca usa el nombre original del usuario
/// como nombre de archivo.
/// </summary>
public interface IAlmacenamientoArchivos
{
    /// <summary>Guarda el contenido y devuelve la ruta relativa del archivo almacenado.</summary>
    Task<string> GuardarAsync(Stream contenido, string nombreFisico, CancellationToken cancellationToken = default);

    /// <summary>Abre el archivo almacenado en la ruta relativa indicada.</summary>
    Task<Stream> AbrirAsync(string ruta, CancellationToken cancellationToken = default);

    /// <summary>Elimina el archivo almacenado; se usa para revertir una subida fallida.</summary>
    Task EliminarAsync(string ruta, CancellationToken cancellationToken = default);
}
