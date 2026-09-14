import { useQuery } from '@tanstack/react-query';
import { getResumenDashboard } from '../../services/dashboardService';
import type { FiltroAsignacion } from '../../services/solicitudService';

export function useResumenDashboard(asignacion?: FiltroAsignacion) {
  return useQuery({ queryKey: ['dashboard', 'resumen', asignacion], queryFn: () => getResumenDashboard(asignacion) });
}
