namespace SB.PortalSolicitudes.Application.Common;

/// <summary>
/// Parametros de paginacion comunes a cualquier listado. Los filtros especificos
/// (<see cref="FiltroSolicitudes"/>, <see cref="FiltroNotificaciones"/>) heredan de esta clase.
/// </summary>
public class ParametrosPaginacion
{
    public const int PAGINA_MINIMA = 1;
    public const int TAMANO_PAGINA_PREDETERMINADO = 20;
    public const int TAMANO_PAGINA_MAXIMO = 100;

    public int Pagina { get; set; } = PAGINA_MINIMA;

    public int TamanoPagina { get; set; } = TAMANO_PAGINA_PREDETERMINADO;
}
