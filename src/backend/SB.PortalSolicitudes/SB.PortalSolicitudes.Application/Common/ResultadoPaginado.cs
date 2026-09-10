namespace SB.PortalSolicitudes.Application.Common;

/// <summary>
/// Pagina de resultados devuelta por los repositorios de listado. Los valores de
/// paginacion se acotan en el constructor para que una peticion maliciosa no fuerce
/// una pagina sin limite (ver <see cref="ParametrosPaginacion"/>).
/// </summary>
public class ResultadoPaginado<T>
{
    public IReadOnlyList<T> Elementos { get; }

    public int TotalElementos { get; }

    public int Pagina { get; }

    public int TamanoPagina { get; }

    public int TotalPaginas { get; }

    public bool TienePaginaAnterior => Pagina > ParametrosPaginacion.PAGINA_MINIMA;

    public bool TienePaginaSiguiente => Pagina < TotalPaginas;

    public ResultadoPaginado(IReadOnlyList<T> elementos, int totalElementos, int pagina, int tamanoPagina)
    {
        Pagina = Math.Max(pagina, ParametrosPaginacion.PAGINA_MINIMA);
        TamanoPagina = Math.Clamp(
            tamanoPagina,
            ParametrosPaginacion.PAGINA_MINIMA,
            ParametrosPaginacion.TAMANO_PAGINA_MAXIMO);

        Elementos = elementos;
        TotalElementos = totalElementos;
        TotalPaginas = TamanoPagina == 0 ? 0 : (int)Math.Ceiling(totalElementos / (double)TamanoPagina);
    }
}
