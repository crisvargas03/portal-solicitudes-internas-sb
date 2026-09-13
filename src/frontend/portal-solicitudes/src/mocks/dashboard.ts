import type { ResumenDashboard, Solicitud } from '../types';
import { ESTADOS_SOLICITUD, PRIORIDADES } from './catalogos';

/** Agrega un conjunto de solicitudes al mismo shape que ResumenDashboardDto. */
export function construirResumenDashboard(solicitudes: Solicitud[]): ResumenDashboard {
  const porEstado = ESTADOS_SOLICITUD.map((estado) => ({
    codigoEstado: estado.codigo,
    nombreEstado: estado.nombre,
    cantidad: solicitudes.filter((solicitud) => solicitud.estado?.codigo === estado.codigo).length,
  }));

  const porPrioridad = PRIORIDADES.map((prioridad) => ({
    prioridadId: prioridad.id,
    nombrePrioridad: prioridad.nombre,
    cantidad: solicitudes.filter((solicitud) => solicitud.prioridadId === prioridad.id).length,
  }));

  const recientes = [...solicitudes].sort((a, b) => b.fechaCreacion.localeCompare(a.fechaCreacion)).slice(0, 5);

  return {
    porEstado,
    porPrioridad,
    totalVencidas: solicitudes.filter((solicitud) => solicitud.estaVencida).length,
    totalSolicitudes: solicitudes.length,
    recientes,
  };
}
