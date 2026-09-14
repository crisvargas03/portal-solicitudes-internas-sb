import type { ReactNode } from 'react';
import { useState } from 'react';
import { ArrowDown, ArrowUp } from 'lucide-react';
import { Pagination } from './Pagination';
import type { PaginacionProps } from './Pagination';

export interface OrdenTabla {
	clave: string;
	direccion: 'asc' | 'desc';
}

export interface Columna<T> {
	clave: string;
	encabezado: string;
	/** Si se omite, renderiza fila[clave] tal cual. */
	render?: (fila: T) => ReactNode;
	ordenable?: boolean;
	alineacion?: 'izquierda' | 'derecha';
	ancho?: string;
}

const TAMANO_PAGINA_POR_DEFECTO = 10;

interface DataTableProps<T> {
	columnas: Columna<T>[];
	filas: T[];
	obtenerClave: (fila: T) => string | number;
	orden?: OrdenTabla;
	onOrdenChange?: (orden: OrdenTabla) => void;
	paginacion?: PaginacionProps;

	tamanoPagina?: number;
	cargando?: boolean;
	mensajeVacio?: string;
	onFilaClick?: (fila: T) => void;
}

export function DataTable<T>({
	columnas,
	filas,
	obtenerClave,
	orden,
	onOrdenChange,
	paginacion,
	tamanoPagina = TAMANO_PAGINA_POR_DEFECTO,
	cargando,
	mensajeVacio = 'Sin resultados',
	onFilaClick,
}: DataTableProps<T>) {
	const [paginaCliente, setPaginaCliente] = useState(1);

	const paginaEsDeServidor = paginacion !== undefined;
	const totalPaginasCliente = Math.max(
		1,
		Math.ceil(filas.length / tamanoPagina),
	);
	const paginaActualCliente = Math.min(paginaCliente, totalPaginasCliente);

	const filasVisibles = paginaEsDeServidor
		? filas
		: filas.slice(
				(paginaActualCliente - 1) * tamanoPagina,
				paginaActualCliente * tamanoPagina,
			);

	const paginacionEfectiva: PaginacionProps | undefined = paginaEsDeServidor
		? paginacion
		: {
				pagina: paginaActualCliente,
				totalPaginas: totalPaginasCliente,
				onPaginaChange: setPaginaCliente,
			};

	function manejarClicEncabezado(columna: Columna<T>) {
		if (!columna.ordenable || !onOrdenChange) return;
		const direccion: OrdenTabla['direccion'] =
			orden?.clave === columna.clave && orden.direccion === 'asc'
				? 'desc'
				: 'asc';
		onOrdenChange({ clave: columna.clave, direccion });
	}

	return (
		<div className='flex flex-col gap-3'>
			<div className='overflow-x-auto rounded-md border border-slate-200'>
				<table className='w-full text-left text-sm'>
					<thead className='bg-gray-bg text-xs font-semibold uppercase tracking-wide text-slate-500'>
						<tr>
							{columnas.map(columna => (
								<th
									key={columna.clave}
									style={{ width: columna.ancho }}
									className={`px-4 py-3 ${columna.alineacion === 'derecha' ? 'text-right' : 'text-left'} ${
										columna.ordenable
											? 'cursor-pointer select-none hover:text-navy'
											: ''
									}`}
									onClick={() =>
										manejarClicEncabezado(columna)
									}>
									<span className='inline-flex items-center gap-1'>
										{columna.encabezado}
										{columna.ordenable &&
											orden?.clave === columna.clave &&
											(orden.direccion === 'asc' ? (
												<ArrowUp size={12} />
											) : (
												<ArrowDown size={12} />
											))}
									</span>
								</th>
							))}
						</tr>
					</thead>
					<tbody className='divide-y divide-slate-200'>
						{cargando ? (
							<tr>
								<td
									colSpan={columnas.length}
									className='px-4 py-8 text-center text-slate-400'>
									Cargando…
								</td>
							</tr>
						) : filasVisibles.length === 0 ? (
							<tr>
								<td
									colSpan={columnas.length}
									className='px-4 py-8 text-center text-slate-400'>
									{mensajeVacio}
								</td>
							</tr>
						) : (
							filasVisibles.map(fila => (
								<tr
									key={obtenerClave(fila)}
									onClick={() => onFilaClick?.(fila)}
									className={`hover:bg-gray-bg/60 ${onFilaClick ? 'cursor-pointer' : ''}`}>
									{columnas.map(columna => (
										<td
											key={columna.clave}
											className={`px-4 py-3 text-slate-600 ${columna.alineacion === 'derecha' ? 'text-right' : ''}`}>
											{columna.render
												? columna.render(fila)
												: String(
														(
															fila as Record<
																string,
																unknown
															>
														)[columna.clave] ?? '',
													)}
										</td>
									))}
								</tr>
							))
						)}
					</tbody>
				</table>
			</div>
			<Pagination {...paginacionEfectiva} />
		</div>
	);
}
