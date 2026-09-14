import type { ReactNode } from 'react';
import { Link } from 'react-router';
import type { Solicitud } from '../../types';
import { TONO_CLASES } from '../ui/tono';
import { obtenerTonoPrioridad } from './prioridadTono';
import { VencimientoIndicator } from './VencimientoIndicator';

interface SolicitudQueueItemProps {
  solicitud: Solicitud;
  accion: ReactNode;
}

function venceHoy(fechaCompromiso: string | null): boolean {
  if (!fechaCompromiso) return false;
  return fechaCompromiso === new Date().toISOString().slice(0, 10);
}

/** Una fila de la cola de Analista: barra de prioridad + texto secundario + acción única del grupo. */
export function SolicitudQueueItem({ solicitud, accion }: SolicitudQueueItemProps) {
  const tonoPrioridad = obtenerTonoPrioridad(solicitud.prioridad.nivel);
  const resaltar = solicitud.estaVencida || venceHoy(solicitud.fechaCompromiso);

  return (
    <li className={`flex items-stretch overflow-hidden rounded-md border border-slate-200 ${resaltar ? 'bg-red-50/40' : 'bg-white'}`}>
      <span className={`w-1 shrink-0 ${TONO_CLASES[tonoPrioridad].punto}`} />
      <div className="flex flex-1 flex-wrap items-center justify-between gap-3 px-4 py-3">
        <div className="min-w-0">
          <Link to={`/solicitudes/${solicitud.id}`} className="block truncate text-sm font-semibold text-navy hover:text-accent-orange">
            {solicitud.titulo}
          </Link>
          <p className="mt-0.5 truncate text-xs text-slate-500">
            {solicitud.codigo} · {solicitud.area.nombre} · {solicitud.solicitante.nombre}
          </p>
        </div>
        <div className="flex shrink-0 items-center gap-4">
          <VencimientoIndicator fechaCompromiso={solicitud.fechaCompromiso} estaVencida={solicitud.estaVencida} />
          {accion}
        </div>
      </div>
    </li>
  );
}
