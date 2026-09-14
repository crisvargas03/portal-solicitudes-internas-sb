import { ChevronLeft, ChevronRight } from 'lucide-react';

export interface PaginacionProps {
  pagina: number;
  totalPaginas: number;
  onPaginaChange: (pagina: number) => void;
}

export function Pagination({ pagina, totalPaginas, onPaginaChange }: PaginacionProps) {
  if (totalPaginas <= 1) return null;

  return (
    <div className="flex items-center justify-between border-t border-slate-200 px-1 pt-3 text-sm text-slate-500">
      <span>
        Página {pagina} de {totalPaginas}
      </span>
      <div className="flex gap-2">
        <button
          type="button"
          disabled={pagina <= 1}
          onClick={() => onPaginaChange(pagina - 1)}
          className="rounded-md border border-slate-300 p-1.5 disabled:opacity-40 hover:border-accent-orange hover:text-accent-orange"
        >
          <ChevronLeft size={16} />
        </button>
        <button
          type="button"
          disabled={pagina >= totalPaginas}
          onClick={() => onPaginaChange(pagina + 1)}
          className="rounded-md border border-slate-300 p-1.5 disabled:opacity-40 hover:border-accent-orange hover:text-accent-orange"
        >
          <ChevronRight size={16} />
        </button>
      </div>
    </div>
  );
}
