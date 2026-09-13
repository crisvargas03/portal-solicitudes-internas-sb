import type { ResumenDashboard } from '../types';
import type { FiltroAsignacion } from './solicitudService';
import { aQueryString, get } from '../lib/apiClient';

/**
 * GET /api/dashboard/resumen: una sola forma de respuesta para los tres roles, con los
 * numeros ya recortados por el alcance del usuario (ADR-0027). `asignacion` es el mismo
 * corte aditivo que el listado — el dashboard de Analista lo usa para que "Mi carga"
 * signifique solo lo suyo, no lo suyo mas lo sin asignar que el alcance tambien deja ver.
 */
export async function getResumenDashboard(asignacion?: FiltroAsignacion): Promise<ResumenDashboard> {
  return get<ResumenDashboard>(`/dashboard/resumen${aQueryString({ asignacion })}`);
}
