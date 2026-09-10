using SB.PortalSolicitudes.Domain.Common;

namespace SB.PortalSolicitudes.Domain.Entities;

/// <summary>
/// Estado del ciclo de vida de una solicitud. Es un catalogo en tabla y no una
/// enumeracion, para poder agregar estados sin recompilar: ver ADR-0004.
/// Las transiciones permitidas viven en <see cref="TransicionPermitida"/>: ver ADR-0005.
/// </summary>
public class EstadoSolicitud : CatalogoBase
{
    public const int MAX_LONGITUD_CODIGO = 40;

    /// <summary>
    /// Clave estable e independiente del identificador de base de datos
    /// (ver <see cref="CodigosEstadoSolicitud"/>). Es la referencia que usan los datos
    /// semilla y cualquier regla, para que los identificadores no se filtren a la logica.
    /// </summary>
    public required string Codigo { get; set; }

    /// <summary>Posicion del estado dentro del flujo, para ordenar listados y el historial.</summary>
    public int Orden { get; set; }

    /// <summary>Indica que el estado cierra el ciclo y no tiene continuacion natural.</summary>
    public bool EsFinal { get; set; }

    public ICollection<Solicitud> Solicitudes { get; set; } = new List<Solicitud>();

    /// <summary>Transiciones que parten de este estado.</summary>
    public ICollection<TransicionPermitida> TransicionesDeSalida { get; set; } = new List<TransicionPermitida>();

    /// <summary>Transiciones que desembocan en este estado.</summary>
    public ICollection<TransicionPermitida> TransicionesDeEntrada { get; set; } = new List<TransicionPermitida>();
}
