namespace Core.ControlAcceso.Domain.Excepciones;

/// <summary>
/// Error de negocio con mensaje controlado para el usuario final (RD-07, RD-08).
/// El controlador lo traduce a una respuesta HTTP con <see cref="CodigoHttp"/>.
/// </summary>
public class ReglaNegocioExcepcion : Exception
{
    public int CodigoHttp { get; }

    public ReglaNegocioExcepcion(string mensaje, int codigoHttp = 400)
        : base(mensaje)
    {
        CodigoHttp = codigoHttp;
    }
}