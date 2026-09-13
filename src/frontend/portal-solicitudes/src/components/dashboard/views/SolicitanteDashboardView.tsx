import { Plus } from 'lucide-react';
import { Link } from 'react-router';
import { useMisSolicitudes } from '../../../hooks/queries/useSolicitudes';
import { useAuthStore } from '../../../store/authStore';
import { CodigosEstadoSolicitud } from '../../../types';
import { SolicitudCard } from '../../solicitudes/SolicitudCard';
import { buttonClasses } from '../../ui/buttonClasses';
import { Card } from '../../ui/Card';
import { MetricCard } from '../../ui/MetricCard';
import { METRICAS_CONFIG } from '../metricasConfig';

/** Solicitante: solo sus propias métricas, con "Nueva solicitud" como acción principal. */
export function SolicitanteDashboardView() {
  const usuario = useAuthStore((state) => state.user);
  const { data: solicitudes = [] } = useMisSolicitudes(usuario?.id ?? 0);

  const abiertas = solicitudes.filter((solicitud) => solicitud.estado?.codigo !== CodigosEstadoSolicitud.CERRADA);
  const recientes = [...solicitudes].sort((a, b) => b.fechaCreacion.localeCompare(a.fechaCreacion)).slice(0, 3);

  return (
    <div className="flex flex-col gap-8">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div className="grid flex-1 grid-cols-1 gap-4 sm:grid-cols-2">
          <MetricCard {...METRICAS_CONFIG.abiertas} etiqueta="Mis solicitudes abiertas" valor={abiertas.length} />
          <MetricCard {...METRICAS_CONFIG.recientes} etiqueta="Total de solicitudes" valor={solicitudes.length} />
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
