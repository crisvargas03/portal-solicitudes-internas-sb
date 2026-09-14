import type { ReactNode } from 'react';
import { create } from 'zustand';

export interface OpcionesConfirmacion {
  titulo: string;
  mensaje: ReactNode;
  textoConfirmar?: string;
  textoCancelar?: string;
  variante?: 'default' | 'peligro';
  /** Si se pasa, el diálogo la ejecuta al confirmar y se queda abierto (botones deshabilitados,
   * spinner en Confirmar) hasta que la promesa resuelve. Si se omite, resuelve al hacer clic
   * (caso "Cerrar sesión", que no tiene petición que esperar). */
  accion?: () => Promise<unknown>;
}

interface SolicitudConfirmacion extends OpcionesConfirmacion {
  resolver: (valor: boolean) => void;
  rechazar: (error: unknown) => void;
}

interface ConfirmState {
  solicitud: SolicitudConfirmacion | null;
  cargando: boolean;
  abrir: (opciones: SolicitudConfirmacion) => void;
  setCargando: (cargando: boolean) => void;
  cerrar: () => void;
}

/**
 * No hay React Context en este proyecto (ver ADR-0033): el diálogo se coordina con este store de
 * Zustand, igual que authStore/uiStore, y un único <ConfirmDialogHost /> montado en App.tsx lo lee.
 */
export const useConfirmStore = create<ConfirmState>((set) => ({
  solicitud: null,
  cargando: false,
  abrir: (solicitud) => set({ solicitud, cargando: false }),
  setCargando: (cargando) => set({ cargando }),
  cerrar: () => set({ solicitud: null, cargando: false }),
}));
