import { useMemo, useState } from 'react';
import { FileText, Plus } from 'lucide-react';
import { Link } from 'react-router';
import { useMisSolicitudes } from '../../../hooks/queries/useSolicitudes';
import { useAuthStore } from '../../../store/authStore';
import { buttonClasses } from '../../ui/buttonClasses';
import { EmptyState } from '../../ui/EmptyState';
import { SolicitudCard } from '../SolicitudCard';
import { SolicitudFilters } from '../SolicitudFilters';
import { FILTROS_VACIOS, convertirAFiltrosSolicitudes } from '../filtrosSolicitudes';
import type { ValoresFiltrosSolicitudes } from '../filtrosSolicitudes';

/** Solicitante: solo sus propias solicitudes, vista ligera en tarjetas — no la tabla densa de administración. */
export function SolicitanteSolicitudesView() {
  const usuario = useAuthStore((state) => state.user);
  const [valoresFiltros, setValoresFiltros] = useState<ValoresFiltrosSolicitudes>(FILTROS_VACIOS);
  const filtros = useMemo(() => convertirAFiltrosSolicitudes(valoresFiltros), [valoresFiltros]);
  const { data: solicitudes = [] } = useMisSolicitudes(usuario?.id ?? 0, filtros);

  return (
    <div className="flex flex-col gap-6">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <SolicitudFilters campos={['estado', 'prioridad', 'rangoFecha']} valores={valoresFiltros} onChange={setValoresFiltros} />
        <Link to="/solicitudes/nueva" className={buttonClasses('primario')}>
          <Plus size={16} />
          Nueva solicitud
        </Link>
      </div>

      {solicitudes.length === 0 ? (
        <EmptyState
          icono={FileText}
          titulo="Aún no tienes solicitudes"
          descripcion="Registra una nueva solicitud para darle seguimiento aquí."
        />
      ) : (
        <div className="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-3">
          {solicitudes.map((solicitud) => (
            <SolicitudCard key={solicitud.id} solicitud={solicitud} />
          ))}
        </div>
      )}
    </div>
  );
}
