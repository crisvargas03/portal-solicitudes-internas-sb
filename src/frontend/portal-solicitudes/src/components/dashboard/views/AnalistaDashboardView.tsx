import { useResumenDashboard } from '../../../hooks/queries/useDashboard';
import { CodigosEstadoSolicitud } from '../../../types';
import { obtenerTonoPrioridad } from '../../solicitudes/prioridadTono';
import { Card } from '../../ui/Card';
import { CountList } from '../../ui/CountList';
import { MetricCard } from '../../ui/MetricCard';
import { METRICAS_CONFIG } from '../metricasConfig';

/**
 * Analista: carga de trabajo personal — no métricas de la organización completa.
 * `asignacion=Asignadas` (ADR-0027) acota los números a lo suyo, dentro del alcance más
 * amplio (suyo + sin asignar) que el rol ya deja ver — "Mi carga" no debe incluir el pool.
 */
export function AnalistaDashboardView() {
  const { data: resumen } = useResumenDashboard('Asignadas');

  const abiertas =
    resumen?.porEstado
      .filter((item) => item.codigoEstado !== CodigosEstadoSolicitud.CERRADA)
      .reduce((total, item) => total + item.cantidad, 0) ?? 0;

  const porPrioridad = (resumen?.porPrioridad ?? []).map((item) => ({
    etiqueta: item.nombrePrioridad,
    valor: item.cantidad,
    tono: obtenerTonoPrioridad(item.prioridadId),
  }));

  return (
    <div className="flex flex-col gap-8">
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-4">
        <MetricCard {...METRICAS_CONFIG.abiertas} etiqueta="Mis solicitudes abiertas" valor={abiertas} />
        <MetricCard {...METRICAS_CONFIG.vencidas} etiqueta="Vencidas" valor={resumen?.totalVencidas ?? 0} />
        <MetricCard {...METRICAS_CONFIG.recientes} etiqueta="Total asignadas" valor={resumen?.totalSolicitudes ?? 0} />
        <MetricCard {...METRICAS_CONFIG.disponibles} etiqueta="Disponibles para tomar" valor={resumen?.totalSinAsignar ?? 0} />
      </div>

      <Card titulo="Mi carga por prioridad">
        <CountList items={porPrioridad} />
      </Card>
    </div>
  );
}
