import type { ReactNode } from 'react';
import { useEffect, useRef } from 'react';
import { createPortal } from 'react-dom';
import { X } from 'lucide-react';

interface ModalProps {
  abierto: boolean;
  titulo: string;
  onCerrar: () => void;
  children: ReactNode;
}

/**
 * Primer componente modal del proyecto (ver ADR-0031): headless, sin dependencias externas.
 * Cierra con Escape o clic en el backdrop; el foco inicial va al panel para que el lector de
 * pantalla anuncie el título de una vez.
 */
export function Modal({ abierto, titulo, onCerrar, children }: ModalProps) {
  const panelRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (!abierto) return;

    panelRef.current?.focus();

    function manejarTecla(evento: KeyboardEvent) {
      if (evento.key === 'Escape') onCerrar();
    }

    document.addEventListener('keydown', manejarTecla);
    return () => document.removeEventListener('keydown', manejarTecla);
  }, [abierto, onCerrar]);

  if (!abierto) return null;

  return createPortal(
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-navy/40 p-4" onClick={onCerrar}>
      <div
        ref={panelRef}
        role="dialog"
        aria-modal="true"
        aria-labelledby="modal-titulo"
        tabIndex={-1}
        className="w-full max-w-md rounded-md bg-white p-5 shadow-lg outline-none"
        onClick={(evento) => evento.stopPropagation()}
      >
        <div className="mb-4 flex items-center justify-between">
          <h3 id="modal-titulo" className="text-sm font-semibold text-navy">
            {titulo}
          </h3>
          <button type="button" onClick={onCerrar} aria-label="Cerrar" className="text-slate-400 hover:text-slate-600">
            <X size={18} />
          </button>
        </div>
        {children}
      </div>
    </div>,
    document.body,
  );
}
