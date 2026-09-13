import { useMemo, useState } from 'react';
import type { OrdenTabla } from '../components/ui/DataTable';

interface OpcionesTablaLocal<T> {
  filas: T[];
  ordenInicial?: OrdenTabla;
  tamanoPagina?: number;
  /** Por defecto lee fila[clave]; se puede pasar para ordenar por un valor derivado. */
  obtenerValorOrden?: (fila: T, clave: string) => string | number;
}

/**
 * Ordena y pagina un arreglo en memoria con la misma forma que espera <DataTable>.
 * Es la única pieza que se elimina cuando el ordenamiento/paginado pase al backend
 * (ResultadoPaginado<T>) — el resto de la vista no cambia.
 */
export function useTablaLocal<T>({ filas, ordenInicial, tamanoPagina = 10, obtenerValorOrden }: OpcionesTablaLocal<T>) {
  const [orden, setOrden] = useState<OrdenTabla | undefined>(ordenInicial);
  const [pagina, setPagina] = useState(1);

  const filasOrdenadas = useMemo(() => {
    if (!orden) return filas;
    const leerValor = obtenerValorOrden ?? ((fila: T, clave: string) => (fila as Record<string, unknown>)[clave] as string | number);
    const copia = [...filas];
    copia.sort((a, b) => {
      const valorA = leerValor(a, orden.clave);
      const valorB = leerValor(b, orden.clave);
      const comparacion = valorA < valorB ? -1 : valorA > valorB ? 1 : 0;
      return orden.direccion === 'asc' ? comparacion : -comparacion;
    });
    return copia;
  }, [filas, orden, obtenerValorOrden]);

  const totalPaginas = Math.max(1, Math.ceil(filasOrdenadas.length / tamanoPagina));
  const paginaSegura = Math.min(pagina, totalPaginas);
  const filasPagina = filasOrdenadas.slice((paginaSegura - 1) * tamanoPagina, paginaSegura * tamanoPagina);

  function manejarOrdenChange(nuevoOrden: OrdenTabla) {
    setOrden(nuevoOrden);
    setPagina(1);
  }

  return {
    filas: filasPagina,
    orden,
    onOrdenChange: manejarOrdenChange,
    paginacion: { pagina: paginaSegura, totalPaginas, onPaginaChange: setPagina },
  };
}
