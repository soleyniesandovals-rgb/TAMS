using Core.ControlAcceso.Domain.Entidades;

namespace Core.ControlAcceso.Application.Interfaces;

public interface ICorreosEnColaRepositorio
{
    Task AgregarAsync(CorreoEnCola correo, CancellationToken cancellationToken = default);
}