import { useMemo, useState } from 'react';
import { FileText, Plus } from 'lucide-react';
import { Link } from 'react-router';
import { useMisSolicitudes } from '../../../hooks/queries/useSolicitudes';
import { buttonClasses } from '../../ui/buttonClasses';
import { EmptyState } from '../../ui/EmptyState';
import { Pagination } from '../../ui/Pagination';
import { SolicitudCard } from '../SolicitudCard';
import { SolicitudFilters } from '../SolicitudFilters';
import { FILTROS_VACIOS, convertirAFiltrosSolicitudes } from '../filtrosSolicitudes';
import type { ValoresFiltrosSolicitudes } from '../filtrosSolicitudes';

const TAMANO_PAGINA = 12;

/** Solicitante: solo sus propias solicitudes (el alcance del rol ya las acota, ver ADR-0012), vista ligera en tarjetas. */
export function SolicitanteSolicitudesView() {
  const [valoresFiltros, setValoresFiltros] = useState<ValoresFiltrosSolicitudes>(FILTROS_VACIOS);
  const [pagina, setPagina] = useState(1);
  const filtros = useMemo(
    () => ({ ...convertirAFiltrosSolicitudes(valoresFiltros), pagina, tamanoPagina: TAMANO_PAGINA }),
    [valoresFiltros, pagina],
  );
  const { data, isLoading } = useMisSolicitudes(filtros);
  const solicitudes = data?.elementos ?? [];

  function manejarFiltrosChange(valores: ValoresFiltrosSolicitudes) {
    setValoresFiltros(valores);
    setPagina(1);
  }

  return (
    <div className="flex flex-col gap-6">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <SolicitudFilters campos={['estado', 'prioridad', 'rangoFecha']} valores={valoresFiltros} onChange={manejarFiltrosChange} />
        <Link to="/solicitudes/nueva" className={buttonClasses('primario')}>
          <Plus size={16} />
          Nueva solicitud
        </Link>
      </div>

      {!isLoading && solicitudes.length === 0 ? (
        <EmptyState
          icono={FileText}
          titulo="Aún no tienes solicitudes"
          descripcion="Registra una nueva solicitud para darle seguimiento aquí."
        />
      ) : (
        <>
          <div className="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-3">
            {solicitudes.map((solicitud) => (
              <SolicitudCard key={solicitud.id} solicitud={solicitud} />
            ))}
          </div>
          {data && <Pagination pagina={data.pagina} totalPaginas={data.totalPaginas} onPaginaChange={setPagina} />}
        </>
      )}
    </div>
  );
}
