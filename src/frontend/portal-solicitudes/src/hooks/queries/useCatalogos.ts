import { useQuery } from '@tanstack/react-query';
import { getAreas, getEstadosSolicitud, getPrioridades, getTiposSolicitud } from '../../services/catalogoService';
import { useAuthStore } from '../../store/authStore';

// Areas, tipos, prioridades y estados casi no cambian (son catalogos de configuracion, no datos
// operativos) — MS_CATALOGO_FRESCO evita cualquier refetch mientras dure la sesion en la pestaña,
// y MS_CATALOGO_EN_CACHE mantiene el resultado servible entre navegaciones aunque el query se desmonte.
const MS_CATALOGO_FRESCO = Infinity;
const MS_CATALOGO_EN_CACHE = 24 * 60 * 60 * 1000; // 24 horas

function useSesionActiva(): boolean {
  return useAuthStore((state) => state.token !== null);
}

export function useAreas() {
  const haySesion = useSesionActiva();
  return useQuery({
    queryKey: ['catalogos', 'areas'],
    queryFn: getAreas,
    enabled: haySesion,
    staleTime: MS_CATALOGO_FRESCO,
    gcTime: MS_CATALOGO_EN_CACHE,
  });
}

export function useTiposSolicitud() {
  const haySesion = useSesionActiva();
  return useQuery({
    queryKey: ['catalogos', 'tiposSolicitud'],
    queryFn: getTiposSolicitud,
    enabled: haySesion,
    staleTime: MS_CATALOGO_FRESCO,
    gcTime: MS_CATALOGO_EN_CACHE,
  });
}

export function usePrioridades() {
  const haySesion = useSesionActiva();
  return useQuery({
    queryKey: ['catalogos', 'prioridades'],
    queryFn: getPrioridades,
    enabled: haySesion,
    staleTime: MS_CATALOGO_FRESCO,
    gcTime: MS_CATALOGO_EN_CACHE,
  });
}

export function useEstadosSolicitud() {
  const haySesion = useSesionActiva();
  return useQuery({
    queryKey: ['catalogos', 'estadosSolicitud'],
    queryFn: getEstadosSolicitud,
    enabled: haySesion,
    staleTime: MS_CATALOGO_FRESCO,
    gcTime: MS_CATALOGO_EN_CACHE,
  });
}
