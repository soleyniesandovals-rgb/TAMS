namespace Tams.Negocio.Application.Excepciones;

/// <summary>
/// El proveedor de feriados no pudo responder correctamente. El mensaje es apto para
/// el usuario final y nunca contiene la clave ni la URL consultada.
/// </summary>
public class ServicioFeriadosNoDisponibleExcepcion : Exception
{
    public ServicioFeriadosNoDisponibleExcepcion()
        : base("El servicio de feriados no está disponible en este momento. Inténtalo más tarde.")
    {
    }

    public ServicioFeriadosNoDisponibleExcepcion(Exception innerException)
        : base("El servicio de feriados no está disponible en este momento. Inténtalo más tarde.", innerException)
    {
    }
}
