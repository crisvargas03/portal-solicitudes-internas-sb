import type { ReactNode } from 'react';
import { Button } from './Button';
import { Modal } from './Modal';

interface ConfirmDialogProps {
  abierto: boolean;
  titulo: string;
  mensaje: ReactNode;
  textoConfirmar?: string;
  textoCancelar?: string;
  variante?: 'default' | 'peligro';
  cargando?: boolean;
  onConfirmar: () => void;
  onCancelar: () => void;
}

/** Diálogo de confirmación genérico sobre `Modal` (ver ADR-0033). `variante='peligro'` usa el
 * mismo tono rojo que ya establecen los banners de error de formulario. */
export function ConfirmDialog({
  abierto,
  titulo,
  mensaje,
  textoConfirmar = 'Confirmar',
  textoCancelar = 'Cancelar',
  variante = 'default',
  cargando = false,
  onConfirmar,
  onCancelar,
}: ConfirmDialogProps) {
  return (
    <Modal abierto={abierto} titulo={titulo} onCerrar={onCancelar}>
      <div className="flex flex-col gap-4">
        <p className="text-sm text-slate-600">{mensaje}</p>
        <div className="flex justify-end gap-2">
          <Button type="button" variante="secundario" tamano="sm" onClick={onCancelar} disabled={cargando}>
            {textoCancelar}
          </Button>
          <Button
            type="button"
            variante={variante === 'peligro' ? 'peligro' : 'primario'}
            tamano="sm"
            cargando={cargando}
            onClick={onConfirmar}
          >
            {textoConfirmar}
          </Button>
        </div>
      </div>
    </Modal>
  );
}
