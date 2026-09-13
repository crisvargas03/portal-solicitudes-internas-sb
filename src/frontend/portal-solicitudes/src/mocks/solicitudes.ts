import { CodigosEstadoSolicitud } from '../types';
import type { Solicitud } from '../types';
import { AREAS, PRIORIDADES, TIPOS_SOLICITUD, USUARIOS, obtenerEstadoPorCodigo } from './catalogos';

/** Resuelta y Cerrada no acarrean vencimiento — ya no hay nada pendiente que atrasar. */
const ESTADOS_SIN_VENCIMIENTO: string[] = [CodigosEstadoSolicitud.RESUELTA, CodigosEstadoSolicitud.CERRADA];

function fechaRelativa(dias: number): string {
  const fecha = new Date();
  fecha.setDate(fecha.getDate() + dias);
  return fecha.toISOString().slice(0, 10);
}

function buscarPorId<T extends { id: number }>(lista: T[], id: number): T {
  const encontrado = lista.find((item) => item.id === id);
  if (!encontrado) throw new Error(`No se encontró el id ${id} en el catálogo`);
  return encontrado;
}

interface EspecificacionSolicitud {
  id: number;
  titulo: string;
  descripcion: string;
  estadoCodigo: string;
  prioridadId: number;
  areaId: number;
  tipoSolicitudId: number;
  usuarioSolicitanteId: number;
  usuarioAsignadoId?: number;
  diasCreacion: number;
  /** Días relativos a hoy; negativo = en el pasado (vencida si el estado aún está activo). */
  diasCompromiso?: number;
}

function crearSolicitud(spec: EspecificacionSolicitud): Solicitud {
  const estado = obtenerEstadoPorCodigo(spec.estadoCodigo);
  const fechaCompromiso = spec.diasCompromiso !== undefined ? fechaRelativa(spec.diasCompromiso) : undefined;
  const estaVencida =
    !ESTADOS_SIN_VENCIMIENTO.includes(estado.codigo) && spec.diasCompromiso !== undefined && spec.diasCompromiso < 0;

  return {
    id: spec.id,
    codigo: `SOL-2026-${String(spec.id).padStart(4, '0')}`,
    titulo: spec.titulo,
    descripcion: spec.descripcion,
    fechaCreacion: fechaRelativa(spec.diasCreacion),
    fechaCompromiso,
    prioridadId: spec.prioridadId,
    prioridad: buscarPorId(PRIORIDADES, spec.prioridadId),
    estadoId: estado.id,
    estado,
    areaId: spec.areaId,
    area: buscarPorId(AREAS, spec.areaId),
    tipoSolicitudId: spec.tipoSolicitudId,
    tipoSolicitud: buscarPorId(TIPOS_SOLICITUD, spec.tipoSolicitudId),
    usuarioSolicitanteId: spec.usuarioSolicitanteId,
    usuarioSolicitante: buscarPorId(USUARIOS, spec.usuarioSolicitanteId),
    usuarioAsignadoId: spec.usuarioAsignadoId,
    usuarioAsignado: spec.usuarioAsignadoId ? buscarPorId(USUARIOS, spec.usuarioAsignadoId) : undefined,
    estaVencida,
  };
}

const ESPECIFICACIONES: EspecificacionSolicitud[] = [
  // Asignadas a Carlos Díaz (analista1, id 2) — mezcla de urgencias para probar el orden de la cola.
  {
    id: 1,
    titulo: 'Acceso a sistema de reportes',
    descripcion: 'El usuario requiere acceso al módulo de reportes gerenciales.',
    estadoCodigo: CodigosEstadoSolicitud.EN_ANALISIS,
    prioridadId: 3,
    areaId: 1,
    tipoSolicitudId: 2,
    usuarioSolicitanteId: 4,
    usuarioAsignadoId: 2,
    diasCreacion: -6,
    diasCompromiso: -2,
  },
  {
    id: 2,
    titulo: 'Falla en impresora del área legal',
    descripcion: 'Impresora HP no responde desde ayer.',
    estadoCodigo: CodigosEstadoSolicitud.EN_PROGRESO,
    prioridadId: 4,
    areaId: 5,
    tipoSolicitudId: 3,
    usuarioSolicitanteId: 5,
    usuarioAsignadoId: 2,
    diasCreacion: -3,
    diasCompromiso: 0,
  },
  {
    id: 3,
    titulo: 'Instalación de antivirus corporativo',
    descripcion: 'Equipo nuevo sin antivirus instalado.',
    estadoCodigo: CodigosEstadoSolicitud.EN_PROGRESO,
    prioridadId: 2,
    areaId: 1,
    tipoSolicitudId: 3,
    usuarioSolicitanteId: 6,
    usuarioAsignadoId: 2,
    diasCreacion: -4,
    diasCompromiso: 3,
  },
  {
    id: 4,
    titulo: 'Actualización de datos personales',
    descripcion: 'Cambio de dirección y teléfono de contacto.',
    estadoCodigo: CodigosEstadoSolicitud.EN_ESPERA_SOLICITANTE,
    prioridadId: 1,
    areaId: 3,
    tipoSolicitudId: 5,
    usuarioSolicitanteId: 4,
    usuarioAsignadoId: 2,
    diasCreacion: -8,
  },

  // Asignadas a Lucía Fernández (analista2, id 3).
  {
    id: 5,
    titulo: 'Solicitud de laptop de respaldo',
    descripcion: 'Laptop actual con fallas de batería.',
    estadoCodigo: CodigosEstadoSolicitud.EN_ANALISIS,
    prioridadId: 3,
    areaId: 1,
    tipoSolicitudId: 3,
    usuarioSolicitanteId: 5,
    usuarioAsignadoId: 3,
    diasCreacion: -2,
    diasCompromiso: -1,
  },
  {
    id: 6,
    titulo: 'Consulta sobre política de vacaciones',
    descripcion: 'Duda sobre acumulación de días.',
    estadoCodigo: CodigosEstadoSolicitud.RESUELTA,
    prioridadId: 1,
    areaId: 3,
    tipoSolicitudId: 5,
    usuarioSolicitanteId: 6,
    usuarioAsignadoId: 3,
    diasCreacion: -10,
    diasCompromiso: -5,
  },

  // Disponibles (sin asignar) — visibles para cualquier Analista en "Disponibles para tomar".
  {
    id: 7,
    titulo: 'Renovación de licencia Office 365',
    descripcion: 'Licencia vence este mes.',
    estadoCodigo: CodigosEstadoSolicitud.REGISTRADA,
    prioridadId: 2,
    areaId: 4,
    tipoSolicitudId: 4,
    usuarioSolicitanteId: 4,
    diasCreacion: -1,
    diasCompromiso: 5,
  },
  {
    id: 8,
    titulo: 'Solicitud de equipo para nuevo ingreso',
    descripcion: 'Equipo completo para colaborador que inicia el lunes.',
    estadoCodigo: CodigosEstadoSolicitud.REGISTRADA,
    prioridadId: 4,
    areaId: 3,
    tipoSolicitudId: 3,
    usuarioSolicitanteId: 5,
    diasCreacion: 0,
    diasCompromiso: 1,
  },
  {
    id: 9,
    titulo: 'Acceso a carpeta compartida de auditoría',
    descripcion: 'Permisos de lectura sobre carpeta de red.',
    estadoCodigo: CodigosEstadoSolicitud.REGISTRADA,
    prioridadId: 3,
    areaId: 2,
    tipoSolicitudId: 2,
    usuarioSolicitanteId: 6,
    diasCreacion: -1,
  },
  {
    id: 10,
    titulo: 'Revisión de contrato de proveedor',
    descripcion: 'Requiere validación jurídica antes de firma.',
    estadoCodigo: CodigosEstadoSolicitud.REGISTRADA,
    prioridadId: 2,
    areaId: 5,
    tipoSolicitudId: 5,
    usuarioSolicitanteId: 4,
    diasCreacion: -2,
    diasCompromiso: -3,
  },

  // Historial de María Peña (solicitante1, id 4) — para la vista de Solicitante.
  {
    id: 11,
    titulo: 'Solicitud de certificación laboral',
    descripcion: 'Necesaria para trámite bancario personal.',
    estadoCodigo: CodigosEstadoSolicitud.CERRADA,
    prioridadId: 1,
    areaId: 3,
    tipoSolicitudId: 5,
    usuarioSolicitanteId: 4,
    usuarioAsignadoId: 3,
    diasCreacion: -30,
    diasCompromiso: -25,
  },
  {
    id: 12,
    titulo: 'Cambio de contraseña de dominio',
    descripcion: 'Bloqueo de cuenta tras varios intentos fallidos.',
    estadoCodigo: CodigosEstadoSolicitud.CERRADA,
    prioridadId: 3,
    areaId: 1,
    tipoSolicitudId: 2,
    usuarioSolicitanteId: 4,
    usuarioAsignadoId: 2,
    diasCreacion: -20,
    diasCompromiso: -18,
  },
  {
    id: 13,
    titulo: 'Solicitud de firma electrónica',
    descripcion: 'Requiere certificado para firmar documentos oficiales.',
    estadoCodigo: CodigosEstadoSolicitud.RESUELTA,
    prioridadId: 2,
    areaId: 5,
    tipoSolicitudId: 5,
    usuarioSolicitanteId: 4,
    usuarioAsignadoId: 3,
    diasCreacion: -12,
    diasCompromiso: -7,
  },

  // Historial de Juan Cruz (solicitante2, id 5).
  {
    id: 14,
    titulo: 'Instalación de VPN para teletrabajo',
    descripcion: 'Requiere acceso remoto seguro.',
    estadoCodigo: CodigosEstadoSolicitud.EN_PROGRESO,
    prioridadId: 3,
    areaId: 1,
    tipoSolicitudId: 2,
    usuarioSolicitanteId: 5,
    usuarioAsignadoId: 2,
    diasCreacion: -5,
    diasCompromiso: 2,
  },
  {
    id: 15,
    titulo: 'Consulta sobre viáticos',
    descripcion: 'Duda sobre reembolso de viaje reciente.',
    estadoCodigo: CodigosEstadoSolicitud.CERRADA,
    prioridadId: 1,
    areaId: 4,
    tipoSolicitudId: 5,
    usuarioSolicitanteId: 5,
    usuarioAsignadoId: 3,
    diasCreacion: -40,
    diasCompromiso: -35,
  },

  // Disponible, crítica y vence hoy — caso límite para la cola de Analista.
  {
    id: 16,
    titulo: 'Caída de sistema de nómina',
    descripcion: 'Sistema de nómina inaccesible para todo el personal.',
    estadoCodigo: CodigosEstadoSolicitud.EN_ANALISIS,
    prioridadId: 4,
    areaId: 1,
    tipoSolicitudId: 1,
    usuarioSolicitanteId: 6,
    diasCreacion: 0,
    diasCompromiso: 0,
  },
];

export const MOCK_SOLICITUDES: Solicitud[] = ESPECIFICACIONES.map(crearSolicitud);
