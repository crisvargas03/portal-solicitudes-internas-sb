import type { ReactNode } from 'react';
import { useEffect, useId, useRef } from 'react';
import { createPortal } from 'react-dom';
import { X } from 'lucide-react';

interface ModalProps {
  abierto: boolean;
  titulo: string;
  onCerrar: () => void;
  children: ReactNode;
}

// Cuenta cuántos Modal están montados y abiertos a la vez, para que un anidamiento (p. ej. un
// ConfirmDialog sobre CambiarEstadoModal) solo cierre el de más arriba con Escape en vez de a
// ambos con una sola pulsación.
let modalesAbiertos = 0;

/**
 * Primer componente modal del proyecto (ver ADR-0031): headless, sin dependencias externas.
 * Cierra con Escape o clic en el backdrop; el foco inicial va al panel para que el lector de
 * pantalla anuncie el título de una vez. Soporta anidamiento: Escape solo cierra el modal
 * montado más recientemente (ver ADR-0033).
 */
export function Modal({ abierto, titulo, onCerrar, children }: ModalProps) {
  const panelRef = useRef<HTMLDivElement>(null);
  const idTitulo = useId();
  const nivelRef = useRef(0);

  useEffect(() => {
    if (!abierto) return;

    modalesAbiertos += 1;
    nivelRef.current = modalesAbiertos;
    panelRef.current?.focus();

    function manejarTecla(evento: KeyboardEvent) {
      if (evento.key === 'Escape' && nivelRef.current === modalesAbiertos) onCerrar();
    }

    document.addEventListener('keydown', manejarTecla);
    return () => {
      document.removeEventListener('keydown', manejarTecla);
      modalesAbiertos -= 1;
    };
  }, [abierto, onCerrar]);

  if (!abierto) return null;

  return createPortal(
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-navy/40 p-4" onClick={onCerrar}>
      <div
        ref={panelRef}
        role="dialog"
        aria-modal="true"
        aria-labelledby={idTitulo}
        tabIndex={-1}
        className="w-full max-w-md rounded-md bg-white p-5 shadow-lg outline-none"
        onClick={(evento) => evento.stopPropagation()}
      >
        <div className="mb-4 flex items-center justify-between">
          <h3 id={idTitulo} className="text-sm font-semibold text-navy">
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
