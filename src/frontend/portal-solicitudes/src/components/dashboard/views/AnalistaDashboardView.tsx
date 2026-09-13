import { useMemo } from 'react';
import { useSolicitudesAsignadas } from '../../../hooks/queries/useSolicitudes';
import { useAuthStore } from '../../../store/authStore';
import { PRIORIDADES } from '../../../mocks/catalogos';
import { CodigosEstadoSolicitud } from '../../../types';
import { obtenerTonoPrioridad } from '../../solicitudes/prioridadTono';
import { Card } from '../../ui/Card';
import { CountList } from '../../ui/CountList';
import { MetricCard } from '../../ui/MetricCard';
import { METRICAS_CONFIG } from '../metricasConfig';

/** Analista: carga de trabajo personal — no métricas de la organización completa. */
export function AnalistaDashboardView() {
  const usuario = useAuthStore((state) => state.user);
  const { data: asignadas = [] } = useSolicitudesAsignadas(usuario?.id ?? 0);

  const abiertas = useMemo(
    () => asignadas.filter((solicitud) => solicitud.estado?.codigo !== CodigosEstadoSolicitud.CERRADA),
    [asignadas],
  );
  const vencidas = useMemo(() => asignadas.filter((solicitud) => solicitud.estaVencida), [asignadas]);
  const porPrioridad = useMemo(
    () =>
      PRIORIDADES.map((prioridad) => ({
        etiqueta: prioridad.nombre,
        valor: abiertas.filter((solicitud) => solicitud.prioridadId === prioridad.id).length,
        tono: obtenerTonoPrioridad(prioridad.nivel),
      })),
    [abiertas],
  );

  return (
    <div className="flex flex-col gap-8">
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
        <MetricCard {...METRICAS_CONFIG.abiertas} etiqueta="Mis solicitudes abiertas" valor={abiertas.length} />
        <MetricCard {...METRICAS_CONFIG.vencidas} etiqueta="Vencidas" valor={vencidas.length} />
        <MetricCard {...METRICAS_CONFIG.recientes} etiqueta="Total asignadas" valor={asignadas.length} />
      </div>

      <Card titulo="Mi carga por prioridad">
        <CountList items={porPrioridad} />
      </Card>
    </div>
  );
}
