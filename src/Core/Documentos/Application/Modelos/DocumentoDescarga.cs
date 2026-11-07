namespace Core.Documentos.Application.Modelos;

/// <summary>
/// Resultado interno de una descarga: el contenido junto con los datos que el
/// controlador necesita para responder el archivo. No se serializa como JSON.
/// </summary>
public sealed record DocumentoDescarga(Stream Contenido, string Nombre, string TipoContenido);
