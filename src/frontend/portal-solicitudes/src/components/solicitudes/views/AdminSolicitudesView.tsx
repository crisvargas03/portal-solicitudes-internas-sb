import { useMemo, useState } from 'react';
import { Plus } from 'lucide-react';
import { Link } from 'react-router';
import { useSolicitudes } from '../../../hooks/queries/useSolicitudes';
import type { DireccionOrden, OrdenSolicitudes } from '../../../services/solicitudService';
import type { Solicitud } from '../../../types';
import { buttonClasses } from '../../ui/buttonClasses';
import { DataTable } from '../../ui/DataTable';
import type { Columna, OrdenTabla } from '../../ui/DataTable';
import { PriorityBadge } from '../PriorityBadge';
import { StatusBadge } from '../StatusBadge';
import { SolicitudFilters } from '../SolicitudFilters';
import { FILTROS_VACIOS, convertirAFiltrosSolicitudes } from '../filtrosSolicitudes';
import type { ValoresFiltrosSolicitudes } from '../filtrosSolicitudes';

const TAMANO_PAGINA = 10;

/** Espejo de las columnas ordenables hacia el whitelist de orden del servidor (ver ADR-0026). */
const ORDEN_POR_CLAVE: Record<string, OrdenSolicitudes> = {
  codigo: 'Codigo',
  titulo: 'Titulo',
};

const COLUMNAS: Columna<Solicitud>[] = [
  {
    clave: 'codigo',
    encabezado: 'Código',
    ordenable: true,
    render: (fila) => (
      <Link to={`/solicitudes/${fila.id}`} className="font-medium text-accent-orange hover:underline">
        {fila.codigo}
      </Link>
    ),
  },
  { clave: 'titulo', encabezado: 'Título', ordenable: true, render: (fila) => <span className="text-navy">{fila.titulo}</span> },
  { clave: 'estado', encabezado: 'Estado', render: (fila) => <StatusBadge estado={fila.estado} /> },
  { clave: 'prioridad', encabezado: 'Prioridad', render: (fila) => <PriorityBadge prioridad={fila.prioridad} /> },
  { clave: 'area', encabezado: 'Área', render: (fila) => fila.area.nombre },
  { clave: 'solicitante', encabezado: 'Solicitante', render: (fila) => fila.solicitante.nombre },
  { clave: 'responsable', encabezado: 'Responsable', render: (fila) => fila.asignado?.nombre ?? '—' },
];

/** Administrador: oversight total — todas las solicitudes, todos los filtros, tabla genérica. */
export function AdminSolicitudesView() {
  const [valoresFiltros, setValoresFiltros] = useState<ValoresFiltrosSolicitudes>(FILTROS_VACIOS);
  const [orden, setOrden] = useState<OrdenTabla>({ clave: 'codigo', direccion: 'desc' });
  const [pagina, setPagina] = useState(1);

  const filtros = useMemo(
    () => ({
      ...convertirAFiltrosSolicitudes(valoresFiltros),
      orden: ORDEN_POR_CLAVE[orden.clave] ?? 'FechaCreacion',
      direccion: orden.direccion === 'asc' ? ('Asc' as DireccionOrden) : ('Desc' as DireccionOrden),
      pagina,
      tamanoPagina: TAMANO_PAGINA,
    }),
    [valoresFiltros, orden, pagina],
  );

  const { data, isLoading } = useSolicitudes(filtros);

  function manejarFiltrosChange(valores: ValoresFiltrosSolicitudes) {
    setValoresFiltros(valores);
    setPagina(1);
  }

  function manejarOrdenChange(nuevoOrden: OrdenTabla) {
    setOrden(nuevoOrden);
    setPagina(1);
  }

  return (
    <div className="flex flex-col gap-6">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <SolicitudFilters
          campos={['estado', 'prioridad', 'area', 'solicitante', 'responsable', 'rangoFecha']}
          valores={valoresFiltros}
          onChange={manejarFiltrosChange}
        />
        <Link to="/solicitudes/nueva" className={buttonClasses('primario')}>
          <Plus size={16} />
          Nueva solicitud
        </Link>
      </div>

      <DataTable
        columnas={COLUMNAS}
        filas={data?.elementos ?? []}
        obtenerClave={(fila) => fila.id}
        orden={orden}
        onOrdenChange={manejarOrdenChange}
        paginacion={{ pagina: data?.pagina ?? pagina, totalPaginas: data?.totalPaginas ?? 1, onPaginaChange: setPagina }}
        cargando={isLoading}
        mensajeVacio="No hay solicitudes que coincidan con los filtros."
      />
    </div>
  );
}
