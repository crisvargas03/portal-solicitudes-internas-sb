import { CodigosEstadoSolicitud } from '../../types';
import type { Tono } from '../ui/tono';

const TONO_POR_CODIGO: Record<string, Tono> = {
  [CodigosEstadoSolicitud.REGISTRADA]: 'neutral',
  [CodigosEstadoSolicitud.EN_ANALISIS]: 'info',
  [CodigosEstadoSolicitud.EN_PROGRESO]: 'navy',
  [CodigosEstadoSolicitud.EN_ESPERA_SOLICITANTE]: 'advertencia',
  [CodigosEstadoSolicitud.RESUELTA]: 'exito',
  [CodigosEstadoSolicitud.CERRADA]: 'oscuro',
};

/** Un código de catálogo no reconocido cae a neutral en vez de romper la vista. */
export function obtenerTonoEstado(codigo: string): Tono {
  return TONO_POR_CODIGO[codigo] ?? 'neutral';
}
