/** Forma exacta de Application/Common/ResultadoPaginado&lt;T&gt;. */
export interface PaginaResultado<T> {
  elementos: T[];
  totalElementos: number;
  pagina: number;
  tamanoPagina: number;
  totalPaginas: number;
  tienePaginaAnterior: boolean;
  tienePaginaSiguiente: boolean;
}
