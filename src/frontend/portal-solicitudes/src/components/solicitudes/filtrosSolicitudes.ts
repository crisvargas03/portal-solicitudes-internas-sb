import type { FiltrosSolicitudes } from '../../services/solicitudService';

export type CampoFiltro = 'estado' | 'prioridad' | 'area' | 'tipo' | 'solicitante' | 'responsable' | 'rangoFecha';

export interface ValoresFiltrosSolicitudes {
  estadoId: string;
  prioridadId: string;
  areaId: string;
  tipoSolicitudId: string;
  usuarioSolicitanteId: string;
  usuarioAsignadoId: string;
  fechaDesde: string;
  fechaHasta: string;
}

export const FILTROS_VACIOS: ValoresFiltrosSolicitudes = {
  estadoId: '',
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
    estadoId: valores.estadoId ? Number(valores.estadoId) : undefined,
    prioridadId: valores.prioridadId ? Number(valores.prioridadId) : undefined,
    areaId: valores.areaId ? Number(valores.areaId) : undefined,
    tipoSolicitudId: valores.tipoSolicitudId ? Number(valores.tipoSolicitudId) : undefined,
    usuarioSolicitanteId: valores.usuarioSolicitanteId ? Number(valores.usuarioSolicitanteId) : undefined,
    usuarioAsignadoId: valores.usuarioAsignadoId ? Number(valores.usuarioAsignadoId) : undefined,
    fechaCreacionDesde: valores.fechaDesde || undefined,
    fechaCreacionHasta: valores.fechaHasta || undefined,
  };
}
