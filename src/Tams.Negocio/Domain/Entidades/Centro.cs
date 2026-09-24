using Tams.Negocio.Domain.Enums;
using Tams.Negocio.Domain.Interfaces;

namespace Tams.Negocio.Domain.Entidades;

/// <summary>Centro educativo y los tipos técnicos que tiene habilitados.</summary>
public class Centro : ICreacionAuditable
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public TipoCentro TipoCentro { get; set; }

    /// <summary>Tipos técnicos habilitados por el centro (vacío si el centro es Normal).</summary>
    public List<TipoTecnico> TiposTecnicosHabilitados { get; set; } = [];

    public DateTime FechaCreacion { get; set; }
}