import { useCallback } from 'react';
import type { OpcionesConfirmacion } from '../store/confirmStore';
import { useConfirmStore } from '../store/confirmStore';

/**
 * confirm({ titulo, mensaje, variante, accion? }) => Promise<boolean>, sin prop-drilling de un
 * modal state (ver ADR-0033). Cualquier componente puede pedir una confirmación sin montar nada.
 */
export function useConfirm() {
  const abrir = useConfirmStore((state) => state.abrir);

  return useCallback(
    (opciones: OpcionesConfirmacion) =>
      new Promise<boolean>((resolver, rechazar) => {
        abrir({ ...opciones, resolver, rechazar });
      }),
    [abrir],
  );
}
