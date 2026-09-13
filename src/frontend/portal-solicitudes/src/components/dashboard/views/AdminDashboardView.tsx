import { Link } from 'react-router';
import { useResumenDashboard } from '../../../hooks/queries/useSolicitudes';
import { CodigosEstadoSolicitud } from '../../../types';
import { StatusBadge } from '../../solicitudes/StatusBadge';
import { obtenerTonoEstado } from '../../solicitudes/estadoTono';
import { obtenerTonoPrioridad } from '../../solicitudes/prioridadTono';
import { Card } from '../../ui/Card';
import { CountList } from '../../ui/CountList';
import { MetricCard } from '../../ui/MetricCard';
import { METRICAS_CONFIG } from '../metricasConfig';

/** Administrador: métricas de toda la organización — oversight, no una vista de trabajo personal. */
export function AdminDashboardView() {
  const { data: resumen } = useResumenDashboard();

  const abiertas =
    resumen?.porEstado
      .filter((item) => item.codigoEstado !== CodigosEstadoSolicitud.CERRADA)
      .reduce((total, item) => total + item.cantidad, 0) ?? 0;
  const cerradas = resumen?.porEstado.find((item) => item.codigoEstado === CodigosEstadoSolicitud.CERRADA)?.cantidad ?? 0;

  return (
    <div className="flex flex-col gap-8">
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <MetricCard {...METRICAS_CONFIG.abiertas} etiqueta="Abiertas" valor={abiertas} />
        <MetricCard {...METRICAS_CONFIG.cerradas} etiqueta="Cerradas" valor={cerradas} />
        <MetricCard {...METRICAS_CONFIG.vencidas} etiqueta="Vencidas" valor={resumen?.totalVencidas ?? 0} />
        <MetricCard {...METRICAS_CONFIG.recientes} etiqueta="Total de solicitudes" valor={resumen?.totalSolicitudes ?? 0} />
      </div>

      <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
        <Card titulo="Por estado">
          <CountList
            items={(resumen?.porEstado ?? []).map((item) => ({
              etiqueta: item.nombreEstado,
              valor: item.cantidad,
              tono: obtenerTonoEstado(item.codigoEstado),
            }))}
          />
        </Card>
        <Card titulo="Por prioridad">
          <CountList
            items={(resumen?.porPrioridad ?? []).map((item) => ({
              etiqueta: item.nombrePrioridad,
              valor: item.cantidad,
              tono: obtenerTonoPrioridad(item.prioridadId),
            }))}
          />
        </Card>
      </div>

      <Card titulo="Últimas registradas">
        <ul className="flex flex-col divide-y divide-slate-200">
          {(resumen?.recientes ?? []).map((solicitud) => (
            <li key={solicitud.id} className="flex items-center justify-between py-3 text-sm">
              <div>
                <Link to={`/solicitudes/${solicitud.id}`} className="font-medium text-navy hover:text-accent-orange">
                  {solicitud.titulo}
                </Link>
                <p className="text-slate-500">{solicitud.codigo}</p>
              </div>
              <StatusBadge estado={solicitud.estado} />
            </li>
          ))}
        </ul>
      </Card>
    </div>
  );
}
