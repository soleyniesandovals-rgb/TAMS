using Core.Documentos.Application.Interfaces;

namespace Core.Documentos.Infrastructura.Servicios;

/// <summary>
/// Almacenamiento local en disco (RF-DOC-01). La carpeta base se toma de la
/// variable de entorno <c>TAMS_DOCUMENTOS_RUTA</c>; si no está definida, se usa
/// la carpeta <c>almacen</c> bajo el directorio base de la aplicación.
/// </summary>
public sealed class AlmacenamientoArchivosLocal : IAlmacenamientoArchivos
{
    private const string VariableRuta = "TAMS_DOCUMENTOS_RUTA";
    private const string CarpetaPorDefecto = "almacen";

    private readonly string _rutaBase;

    public AlmacenamientoArchivosLocal()
    {
        var rutaConfigurada = Environment.GetEnvironmentVariable(VariableRuta);

        var rutaBase = string.IsNullOrWhiteSpace(rutaConfigurada)
            ? Path.Combine(AppContext.BaseDirectory, CarpetaPorDefecto)
            : rutaConfigurada;

        _rutaBase = Path.TrimEndingDirectorySeparator(Path.GetFullPath(rutaBase));
        Directory.CreateDirectory(_rutaBase);
    }

    public async Task<string> GuardarAsync(
        Stream contenido,
        string nombreFisico,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(contenido);

        var rutaCompleta = ResolverRuta(nombreFisico);

        await using var destino = new FileStream(
            rutaCompleta,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 81920,
            FileOptions.Asynchronous);

        await contenido.CopyToAsync(destino, cancellationToken);
        await destino.FlushAsync(cancellationToken);

        // La base de datos guarda solo el nombre relativo, nunca la ruta absoluta.
        return nombreFisico;
    }

    public Task<Stream> AbrirAsync(string ruta, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var rutaCompleta = ResolverRuta(ruta);
        if (!File.Exists(rutaCompleta))
        {
            throw new FileNotFoundException("El archivo del documento no existe.", rutaCompleta);
        }

        Stream contenido = new FileStream(
            rutaCompleta,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 81920,
            FileOptions.Asynchronous | FileOptions.SequentialScan);

        return Task.FromResult(contenido);
    }

    public Task EliminarAsync(string ruta, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var rutaCompleta = ResolverRuta(ruta);
        if (File.Exists(rutaCompleta))
        {
            File.Delete(rutaCompleta);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Resuelve una ruta relativa dentro de la carpeta base. Rechaza nombres que
    /// intenten salir de ella (RD-07).
    /// </summary>
    private string ResolverRuta(string ruta)
    {
        if (string.IsNullOrWhiteSpace(ruta))
        {
            throw new ArgumentException("La ruta de almacenamiento es obligatoria.", nameof(ruta));
        }

        var nombre = Path.GetFileName(ruta);
        if (!string.Equals(nombre, ruta, StringComparison.Ordinal))
        {
            throw new ArgumentException("La ruta de almacenamiento no es válida.", nameof(ruta));
        }

        var rutaCompleta = Path.GetFullPath(Path.Combine(_rutaBase, nombre));
        var prefijoBase = _rutaBase + Path.DirectorySeparatorChar;

        if (!rutaCompleta.StartsWith(prefijoBase, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("La ruta de almacenamiento no es válida.", nameof(ruta));
        }

        return rutaCompleta;
    }
}
