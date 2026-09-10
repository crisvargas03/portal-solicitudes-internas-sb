namespace SB.PortalSolicitudes.Domain.Entities;

/// <summary>
/// Contador por año para el código legible de <see cref="Solicitud"/>
/// (formato <c>SOL-{Anio}-{Ultimo:D4}</c>, ver ADR-0011). <see cref="Anio"/> es la clave
/// natural: no hereda de <see cref="Common.EntidadBase"/> porque no necesita un Id sustituto.
/// </summary>
public class ContadorCodigo
{
    public int Anio { get; set; }

    public int Ultimo { get; set; }
}
