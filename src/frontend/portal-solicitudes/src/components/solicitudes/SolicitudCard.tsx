import { Link } from 'react-router';
import type { Solicitud } from '../../types';
import { StatusBadge } from './StatusBadge';

interface SolicitudCardProps {
  solicitud: Solicitud;
}

function formatearFecha(fecha: string): string {
  return new Date(`${fecha}T00:00:00`).toLocaleDateString('es-DO', { day: '2-digit', month: 'short', year: 'numeric' });
}

/** Tarjeta de la vista Solicitante — más liviana que una fila de tabla, pensada para lectura personal. */
export function SolicitudCard({ solicitud }: SolicitudCardProps) {
  return (
    <Link
      to={`/solicitudes/${solicitud.id}`}
      className="flex flex-col gap-3 rounded-md border border-slate-200 p-4 transition-colors hover:border-accent-orange"
    >
      <div className="flex items-start justify-between gap-3">
        <div className="min-w-0">
          <p className="truncate text-sm font-semibold text-navy">{solicitud.titulo}</p>
          <p className="mt-0.5 text-xs text-slate-500">{solicitud.codigo}</p>
        </div>
        <StatusBadge estado={solicitud.estado} />
      </div>
      <p className="text-xs text-slate-500">Creada el {formatearFecha(solicitud.fechaCreacion)}</p>
    </Link>
  );
}
