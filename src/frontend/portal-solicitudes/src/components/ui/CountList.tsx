import { TONO_CLASES } from './tono';
import type { Tono } from './tono';

interface CountListItem {
  etiqueta: string;
  valor: number;
  tono: Tono;
}

interface CountListProps {
  items: CountListItem[];
}

/** Lista de conteos con punto de color por categoría (estado, prioridad, etc.). */
export function CountList({ items }: CountListProps) {
  return (
    <ul className="flex flex-col gap-2">
      {items.map((item) => (
        <li
          key={item.etiqueta}
          className="flex items-center justify-between rounded-md border border-slate-200 px-4 py-2.5 text-sm text-slate-700"
        >
          <span className="flex items-center gap-2">
            <span className={`h-2.5 w-2.5 rounded-full ${TONO_CLASES[item.tono].punto}`} />
            {item.etiqueta}
          </span>
          <span className="font-semibold text-navy">{item.valor}</span>
        </li>
      ))}
    </ul>
  );
}
