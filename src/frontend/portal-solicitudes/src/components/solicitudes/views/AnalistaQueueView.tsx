import { useMemo, useState } from 'react';
import { Inbox } from 'lucide-react';
import { toast } from 'sonner';
import { useCambiarAsignacion, useSolicitudesAsignadas, useSolicitudesDisponibles } from '../../../hooks/queries/useSolicitudes';
import { useAuthStore } from '../../../store/authStore';
import { ErrorApi } from '../../../lib/apiClient';
import { Button } from '../../ui/Button';
import { EmptyState } from '../../ui/EmptyState';
import { Pagination } from '../../ui/Pagination';
import { SegmentedControl } from '../../ui/SegmentedControl';
import { CambiarEstadoModal } from '../CambiarEstadoModal';
import { SolicitudQueueItem } from '../SolicitudQueueItem';
import { SolicitudFilters } from '../SolicitudFilters';
import { FILTROS_VACIOS, convertirAFiltrosSolicitudes } from '../filtrosSolicitudes';
import type { ValoresFiltrosSolicitudes } from '../filtrosSolicitudes';

type Grupo = 'asignadas' | 'disponibles';

const TAMANO_PAGINA = 10;

/**
 * Analista: cola de trabajo, no una tabla genérica ni un Kanban. Dos grupos que nunca se
 * mezclan y nunca comparten la misma acción (ADR-0012: asignadas a sí mismo + sin responsable).
 * El orden por urgencia (prioridad, luego vencimiento) lo aplica el servidor (orden=Urgencia,
 * ver ADR-0026) — ya no se reordena en el cliente.
 */
export function AnalistaQueueView() {
  const [grupo, setGrupo] = useState<Grupo>('asignadas');
  const [valoresFiltros, setValoresFiltros] = useState<ValoresFiltrosSolicitudes>(FILTROS_VACIOS);
  const [pagina, setPagina] = useState(1);
  const [solicitudEnCambioDeEstado, setSolicitudEnCambioDeEstado] = useState<number | null>(null);
  const usuarioActualId = useAuthStore((state) => state.user?.id);
  const cambiarAsignacion = useCambiarAsignacion();

  const filtros = useMemo(
    () => ({
      ...convertirAFiltrosSolicitudes(valoresFiltros),
      orden: 'Urgencia' as const,
      pagina,
      tamanoPagina: TAMANO_PAGINA,
    }),
    [valoresFiltros, pagina],
  );

  const { data: asignadas, isLoading: cargandoAsignadas } = useSolicitudesAsignadas(filtros);
  const { data: disponibles, isLoading: cargandoDisponibles } = useSolicitudesDisponibles(filtros);

  const paginaActual = grupo === 'asignadas' ? asignadas : disponibles;
  const filas = paginaActual?.elementos ?? [];
  const cargando = grupo === 'asignadas' ? cargandoAsignadas : cargandoDisponibles;

  function manejarFiltrosChange(valores: ValoresFiltrosSolicitudes) {
    setValoresFiltros(valores);
    setPagina(1);
  }

  function manejarGrupoChange(nuevoGrupo: Grupo) {
    setGrupo(nuevoGrupo);
    setPagina(1);
  }

  /** Reclamar para sí mismo (ADR-0012): el id sale de la sesión, nunca de la fila. */
  async function tomar(solicitudId: number) {
    if (!usuarioActualId) return;

    try {
      await cambiarAsignacion.mutateAsync({ id: solicitudId, usuarioAsignadoId: usuarioActualId });
      toast.success('Solicitud tomada');
    } catch (error) {
      toast.error(error instanceof ErrorApi ? error.message : 'No se pudo tomar la solicitud.');
    }
  }

  return (
    <div className="flex flex-col gap-6">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <SegmentedControl
          valor={grupo}
          onChange={(valor) => manejarGrupoChange(valor as Grupo)}
          opciones={[
            { valor: 'asignadas', etiqueta: 'Asignadas a mí', contador: asignadas?.totalElementos ?? 0 },
            { valor: 'disponibles', etiqueta: 'Disponibles para tomar', contador: disponibles?.totalElementos ?? 0 },
          ]}
        />
        {/* Sin filtro de Solicitante/Responsable: es implícitamente el propio analista. */}
        <SolicitudFilters campos={['estado', 'prioridad']} valores={valoresFiltros} onChange={manejarFiltrosChange} />
      </div>

      {!cargando && filas.length === 0 ? (
        <EmptyState
          icono={Inbox}
          titulo={grupo === 'asignadas' ? 'No tienes solicitudes asignadas' : 'No hay solicitudes disponibles'}
          descripcion={
            grupo === 'asignadas'
              ? 'Las solicitudes que te asignen aparecerán aquí.'
              : 'Todas las solicitudes están asignadas por el momento.'
          }
        />
      ) : (
        <>
          <ul className="flex flex-col gap-2">
            {filas.map((solicitud) => (
              <SolicitudQueueItem
                key={solicitud.id}
                solicitud={solicitud}
                accion={
                  grupo === 'asignadas' ? (
                    <Button
                      type="button"
                      variante="secundario"
                      tamano="sm"
                      onClick={() => setSolicitudEnCambioDeEstado(solicitud.id)}
                    >
                      Cambiar estado
                    </Button>
                  ) : (
                    <Button
                      type="button"
                      tamano="sm"
                      cargando={cambiarAsignacion.isPending && cambiarAsignacion.variables?.id === solicitud.id}
                      onClick={() => tomar(solicitud.id)}
                    >
                      Tomar
                    </Button>
                  )
                }
              />
            ))}
          </ul>
          {paginaActual && (
            <Pagination pagina={paginaActual.pagina} totalPaginas={paginaActual.totalPaginas} onPaginaChange={setPagina} />
          )}
        </>
      )}

      {solicitudEnCambioDeEstado !== null && (
        <CambiarEstadoModal solicitudId={solicitudEnCambioDeEstado} onCerrar={() => setSolicitudEnCambioDeEstado(null)} />
      )}
    </div>
  );
}
