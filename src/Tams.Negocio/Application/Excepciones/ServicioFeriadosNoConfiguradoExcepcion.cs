namespace Tams.Negocio.Application.Excepciones;

/// <summary>
/// El proveedor de feriados no puede consultarse porque falta su configuración.
/// El mensaje es apto para el usuario final y nunca contiene la clave ni la URL.
/// </summary>
public class ServicioFeriadosNoConfiguradoExcepcion : Exception
{
    public ServicioFeriadosNoConfiguradoExcepcion()
        : base("El servicio de feriados no está configurado. Falta la clave de Calendarific.")
    {
    }
}
