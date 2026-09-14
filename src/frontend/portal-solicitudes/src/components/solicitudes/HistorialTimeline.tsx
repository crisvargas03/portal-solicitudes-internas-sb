import { obtenerTonoEstado } from './estadoTono';
import { TONO_CLASES } from '../ui/tono';
import type { HistorialEstadoDetalle } from '../../types';

interface HistorialTimelineProps {
  historial: HistorialEstadoDetalle[];
}

/** Orden cronológico (el más antiguo primero, tal como lo entrega el servidor): se lee como la
 * historia de la solicitud de arriba hacia abajo, terminando en el estado actual. */
export function HistorialTimeline({ historial }: HistorialTimelineProps) {
  if (historial.length === 0) {
    return <p className="text-sm text-slate-500">Sin cambios de estado todavía.</p>;
  }

  return (
    <ol className="border-l-2 border-slate-200 pl-6">
      {historial.map((entrada) => {
        const tono = obtenerTonoEstado(entrada.estadoNuevo.codigo);
        return (
          <li key={entrada.id} className="relative pb-6 last:pb-0">
            <span
              className={`absolute -left-6 top-1 h-3 w-3 -translate-x-1/2 rounded-full ring-4 ring-white ${TONO_CLASES[tono].punto}`}
            />
            <div className="flex flex-wrap items-baseline gap-x-2 gap-y-1">
              <span className="text-sm font-semibold text-navy">{entrada.estadoNuevo.nombre}</span>
              <span className="text-xs text-slate-400">{new Date(entrada.fecha).toLocaleString()}</span>
            </div>
            {entrada.comentario && <p className="mt-1 text-sm text-slate-600">{entrada.comentario}</p>}
            <p className="mt-1 text-xs text-slate-500">{entrada.usuario.nombre}</p>
          </li>
        );
      })}
    </ol>
  );
}
