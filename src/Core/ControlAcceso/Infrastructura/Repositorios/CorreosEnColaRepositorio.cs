using Core.ControlAcceso.Application.Interfaces;
using Core.ControlAcceso.Domain.Entidades;
using Core.ControlAcceso.Infrastructura.Persistence;

namespace Core.ControlAcceso.Infrastructura.Repositorios;

public class CorreosEnColaRepositorio(CoreDbContext context) : ICorreosEnColaRepositorio
{
    public async Task AgregarAsync(CorreoEnCola correo, CancellationToken cancellationToken = default)
        => await context.CorreosEnCola.AddAsync(correo, cancellationToken);
}