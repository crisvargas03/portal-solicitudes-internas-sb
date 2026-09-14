import { CodigosEstadoSolicitud } from '../types';
import type { Area, EstadoSolicitud, Prioridad, TipoSolicitud, Usuario } from '../types';

// Valores alineados con DatosSemillaCatalogos / SeedCommandHandler del backend,
// para que el swap a la API real no cambie ids ni nombres visibles.

export const AREAS: Area[] = [
  { id: 1, nombre: 'Tecnología de la Información' },
  { id: 2, nombre: 'Supervisión Bancaria' },
  { id: 3, nombre: 'Recursos Humanos' },
  { id: 4, nombre: 'Administración y Finanzas' },
  { id: 5, nombre: 'Consultoría Jurídica' },
];

export const TIPOS_SOLICITUD: TipoSolicitud[] = [
  { id: 1, nombre: 'Soporte técnico' },
  { id: 2, nombre: 'Acceso a sistemas' },
  { id: 3, nombre: 'Equipo y hardware' },
  { id: 4, nombre: 'Requerimiento de software' },
  { id: 5, nombre: 'Consulta general' },
];

export const PRIORIDADES: Prioridad[] = [
  { id: 1, nombre: 'Baja', nivel: 1 },
  { id: 2, nombre: 'Media', nivel: 2 },
  { id: 3, nombre: 'Alta', nivel: 3 },
  { id: 4, nombre: 'Crítica', nivel: 4 },
];

export const ESTADOS_SOLICITUD: EstadoSolicitud[] = [
  { id: 1, nombre: 'Registrada', codigo: CodigosEstadoSolicitud.REGISTRADA, orden: 1, esFinal: false },
  { id: 2, nombre: 'En análisis', codigo: CodigosEstadoSolicitud.EN_ANALISIS, orden: 2, esFinal: false },
  { id: 3, nombre: 'En progreso', codigo: CodigosEstadoSolicitud.EN_PROGRESO, orden: 3, esFinal: false },
  {
    id: 4,
    nombre: 'En espera del solicitante',
    codigo: CodigosEstadoSolicitud.EN_ESPERA_SOLICITANTE,
    orden: 4,
    esFinal: false,
  },
  { id: 5, nombre: 'Resuelta', codigo: CodigosEstadoSolicitud.RESUELTA, orden: 5, esFinal: false },
  { id: 6, nombre: 'Cerrada', codigo: CodigosEstadoSolicitud.CERRADA, orden: 6, esFinal: true },
];

export const USUARIOS: Usuario[] = [
  { id: 1, nombre: 'Admin Principal', email: 'admin@portalsolicitudes.test', rol: 'Administrador', activo: true },
  { id: 2, nombre: 'Carlos Díaz', email: 'analista1@portalsolicitudes.test', rol: 'Analista', activo: true },
  { id: 3, nombre: 'Lucía Fernández', email: 'analista2@portalsolicitudes.test', rol: 'Analista', activo: true },
  { id: 4, nombre: 'María Peña', email: 'solicitante1@portalsolicitudes.test', rol: 'Solicitante', activo: true },
  { id: 5, nombre: 'Juan Cruz', email: 'solicitante2@portalsolicitudes.test', rol: 'Solicitante', activo: true },
  { id: 6, nombre: 'Ana Reyes', email: 'solicitante3@portalsolicitudes.test', rol: 'Solicitante', activo: true },
];

export function obtenerEstadoPorCodigo(codigo: string): EstadoSolicitud {
  return ESTADOS_SOLICITUD.find((estado) => estado.codigo === codigo) ?? ESTADOS_SOLICITUD[0];
}
