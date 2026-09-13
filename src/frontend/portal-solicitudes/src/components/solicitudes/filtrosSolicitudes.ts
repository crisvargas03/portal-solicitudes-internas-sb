import type { FiltrosSolicitudes } from '../../services/solicitudService';

export type CampoFiltro = 'estado' | 'prioridad' | 'area' | 'tipo' | 'solicitante' | 'responsable' | 'rangoFecha';

export interface ValoresFiltrosSolicitudes {
  estadoCodigo: string;
  prioridadId: string;
  areaId: string;
  tipoSolicitudId: string;
  usuarioSolicitanteId: string;
  usuarioAsignadoId: string;
  fechaDesde: string;
  fechaHasta: string;
}

export const FILTROS_VACIOS: ValoresFiltrosSolicitudes = {
  estadoCodigo: '',
  prioridadId: '',
  areaId: '',
  tipoSolicitudId: '',
  usuarioSolicitanteId: '',
  usuarioAsignadoId: '',
  fechaDesde: '',
  fechaHasta: '',
};

export function convertirAFiltrosSolicitudes(valores: ValoresFiltrosSolicitudes): FiltrosSolicitudes {
  return {
    estadoCodigo: valores.estadoCodigo || undefined,
    prioridadId: valores.prioridadId ? Number(valores.prioridadId) : undefined,
    areaId: valores.areaId ? Number(valores.areaId) : undefined,
    tipoSolicitudId: valores.tipoSolicitudId ? Number(valores.tipoSolicitudId) : undefined,
    usuarioSolicitanteId: valores.usuarioSolicitanteId ? Number(valores.usuarioSolicitanteId) : undefined,
    usuarioAsignadoId: valores.usuarioAsignadoId ? Number(valores.usuarioAsignadoId) : undefined,
    fechaDesde: valores.fechaDesde || undefined,
    fechaHasta: valores.fechaHasta || undefined,
  };
}
