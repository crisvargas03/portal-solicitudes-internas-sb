import { Plus } from 'lucide-react';
import { Link } from 'react-router';
import { useResumenDashboard } from '../../../hooks/queries/useDashboard';
import { CodigosEstadoSolicitud } from '../../../types';
import { SolicitudCard } from '../../solicitudes/SolicitudCard';
import { buttonClasses } from '../../ui/buttonClasses';
import { Card } from '../../ui/Card';
import { MetricCard } from '../../ui/MetricCard';
import { METRICAS_CONFIG } from '../metricasConfig';

/** Solicitante: solo sus propias métricas (el alcance del rol ya las acota, ver ADR-0012), con "Nueva solicitud" como acción principal. */
export function SolicitanteDashboardView() {
  const { data: resumen } = useResumenDashboard();

  const abiertas =
    resumen?.porEstado
      .filter((item) => item.codigoEstado !== CodigosEstadoSolicitud.CERRADA)
      .reduce((total, item) => total + item.cantidad, 0) ?? 0;
  const recientes = (resumen?.recientes ?? []).slice(0, 3);

  return (
    <div className="flex flex-col gap-8">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div className="grid flex-1 grid-cols-1 gap-4 sm:grid-cols-2">
          <MetricCard {...METRICAS_CONFIG.abiertas} etiqueta="Mis solicitudes abiertas" valor={abiertas} />
          <MetricCard {...METRICAS_CONFIG.recientes} etiqueta="Total de solicitudes" valor={resumen?.totalSolicitudes ?? 0} />
        </div>
        <Link to="/solicitudes/nueva" className={buttonClasses('primario')}>
          <Plus size={16} />
          Nueva solicitud
        </Link>
      </div>

      <Card titulo="Mis últimas solicitudes">
        <div className="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-3">
          {recientes.map((solicitud) => (
            <SolicitudCard key={solicitud.id} solicitud={solicitud} />
          ))}
        </div>
      </Card>
    </div>
  );
}
