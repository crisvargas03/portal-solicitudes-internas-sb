import { CodigosEstadoSolicitud } from '../types';
import type { RolUsuario, TransicionDisponible } from '../types';
import { obtenerEstadoPorCodigo } from './catalogos';

interface DefinicionTransicion {
  desde: string;
  hacia: string;
  roles: RolUsuario[];
  requiereComentario: boolean;
}

// Espejo de ConstruirRolesDeTransiciones (backend): las transiciones son datos, no un
// enum del cliente — nunca se ofrece aquí una transición que el backend no aceptaría.
const DEFINICIONES: DefinicionTransicion[] = [
  { desde: CodigosEstadoSolicitud.REGISTRADA, hacia: CodigosEstadoSolicitud.EN_ANALISIS, roles: ['Administrador', 'Analista'], requiereComentario: false },
  { desde: CodigosEstadoSolicitud.EN_ANALISIS, hacia: CodigosEstadoSolicitud.EN_PROGRESO, roles: ['Administrador', 'Analista'], requiereComentario: false },
  { desde: CodigosEstadoSolicitud.EN_PROGRESO, hacia: CodigosEstadoSolicitud.EN_ESPERA_SOLICITANTE, roles: ['Administrador', 'Analista'], requiereComentario: true },
  { desde: CodigosEstadoSolicitud.EN_PROGRESO, hacia: CodigosEstadoSolicitud.RESUELTA, roles: ['Administrador', 'Analista'], requiereComentario: true },
  { desde: CodigosEstadoSolicitud.EN_ESPERA_SOLICITANTE, hacia: CodigosEstadoSolicitud.EN_PROGRESO, roles: ['Administrador', 'Analista', 'Solicitante'], requiereComentario: false },
  { desde: CodigosEstadoSolicitud.RESUELTA, hacia: CodigosEstadoSolicitud.EN_PROGRESO, roles: ['Administrador', 'Analista'], requiereComentario: true },
  { desde: CodigosEstadoSolicitud.RESUELTA, hacia: CodigosEstadoSolicitud.CERRADA, roles: ['Administrador', 'Analista', 'Solicitante'], requiereComentario: false },
  { desde: CodigosEstadoSolicitud.CERRADA, hacia: CodigosEstadoSolicitud.EN_ANALISIS, roles: ['Administrador', 'Analista'], requiereComentario: true },
];

export function obtenerTransicionesDisponibles(estadoActualCodigo: string, rol: RolUsuario): TransicionDisponible[] {
  return DEFINICIONES.filter((definicion) => definicion.desde === estadoActualCodigo && definicion.roles.includes(rol)).map(
    (definicion) => {
      const estadoDestino = obtenerEstadoPorCodigo(definicion.hacia);
      return {
        estadoDestinoId: estadoDestino.id,
        codigo: estadoDestino.codigo,
        nombre: estadoDestino.nombre,
        requiereComentario: definicion.requiereComentario,
      };
    },
  );
}
