import { useMemo, useState } from 'react';
import { Plus } from 'lucide-react';
import { Link } from 'react-router';
import { useSolicitudes } from '../../../hooks/queries/useSolicitudes';
import { useTablaLocal } from '../../../hooks/useTablaLocal';
import type { Solicitud } from '../../../types';
import { buttonClasses } from '../../ui/buttonClasses';
import { DataTable } from '../../ui/DataTable';
import type { Columna } from '../../ui/DataTable';
import { PriorityBadge } from '../PriorityBadge';
import { StatusBadge } from '../StatusBadge';
import { SolicitudFilters } from '../SolicitudFilters';
import { FILTROS_VACIOS, convertirAFiltrosSolicitudes } from '../filtrosSolicitudes';
import type { ValoresFiltrosSolicitudes } from '../filtrosSolicitudes';

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
  { clave: 'area', encabezado: 'Área', render: (fila) => fila.area?.nombre },
  { clave: 'solicitante', encabezado: 'Solicitante', render: (fila) => fila.usuarioSolicitante?.nombre },
  { clave: 'responsable', encabezado: 'Responsable', render: (fila) => fila.usuarioAsignado?.nombre ?? '—' },
];

/** Administrador: oversight total — todas las solicitudes, todos los filtros, tabla genérica. */
export function AdminSolicitudesView() {
  const [valoresFiltros, setValoresFiltros] = useState<ValoresFiltrosSolicitudes>(FILTROS_VACIOS);
  const filtros = useMemo(() => convertirAFiltrosSolicitudes(valoresFiltros), [valoresFiltros]);
  const { data: solicitudes = [], isLoading } = useSolicitudes(filtros);
  const tabla = useTablaLocal({ filas: solicitudes, ordenInicial: { clave: 'codigo', direccion: 'desc' } });

  return (
    <div className="flex flex-col gap-6">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <SolicitudFilters
          campos={['estado', 'prioridad', 'area', 'solicitante', 'responsable', 'rangoFecha']}
          valores={valoresFiltros}
          onChange={setValoresFiltros}
        />
        <Link to="/solicitudes/nueva" className={buttonClasses('primario')}>
          <Plus size={16} />
          Nueva solicitud
        </Link>
      </div>

      <DataTable
        columnas={COLUMNAS}
        filas={tabla.filas}
        obtenerClave={(fila) => fila.id}
        orden={tabla.orden}
        onOrdenChange={tabla.onOrdenChange}
        paginacion={tabla.paginacion}
        cargando={isLoading}
        mensajeVacio="No hay solicitudes que coincidan con los filtros."
      />
    </div>
  );
}
