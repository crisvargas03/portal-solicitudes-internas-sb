import { useQuery } from '@tanstack/react-query';
import { getSolicitudById, getSolicitudes } from '../../services/solicitudService';

export function useSolicitudes() {
  return useQuery({
    queryKey: ['solicitudes'],
    queryFn: getSolicitudes,
  });
}

export function useSolicitud(id: number) {
  return useQuery({
    queryKey: ['solicitudes', id],
    queryFn: () => getSolicitudById(id),
    enabled: Number.isFinite(id),
  });
}
