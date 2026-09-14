import type { LucideIcon } from 'lucide-react';
import { TONO_CLASES } from './tono';
import type { Tono } from './tono';

interface MetricCardProps {
  icono: LucideIcon;
  etiqueta: string;
  valor: string | number;
  tono: Tono;
  detalle?: string;
}

export function MetricCard({ icono: Icono, etiqueta, valor, tono, detalle }: MetricCardProps) {
  return (
    <div className="flex items-center gap-4 rounded-md border border-slate-200 p-4">
      <span className={`flex h-10 w-10 shrink-0 items-center justify-center rounded-full ${TONO_CLASES[tono].suave}`}>
        <Icono size={20} />
      </span>
      <div>
        <p className="text-xs text-slate-500">{etiqueta}</p>
        <p className="text-xl font-semibold text-navy">{valor}</p>
        {detalle && <p className="text-xs text-slate-400">{detalle}</p>}
      </div>
    </div>
  );
}
