import { useMemo, useState } from 'react';
import { Inbox } from 'lucide-react';
import { useSolicitudesAsignadas, useSolicitudesDisponibles } from '../../../hooks/queries/useSolicitudes';
import { useAuthStore } from '../../../store/authStore';
import type { Solicitud } from '../../../types';
import { buttonClasses } from '../../ui/buttonClasses';
import { EmptyState } from '../../ui/EmptyState';
import { SegmentedControl } from '../../ui/SegmentedControl';
import { SolicitudQueueItem } from '../SolicitudQueueItem';
import { SolicitudFilters } from '../SolicitudFilters';
import { FILTROS_VACIOS, convertirAFiltrosSolicitudes } from '../filtrosSolicitudes';
import type { ValoresFiltrosSolicitudes } from '../filtrosSolicitudes';

type Grupo = 'asignadas' | 'disponibles';

/** Prioridad primero, luego fecha de vencimiento (vencidas/próximas primero, sin fecha al final). */
function ordenarPorUrgencia(solicitudes: Solicitud[]): Solicitud[] {
  return [...solicitudes].sort((a, b) => {
    const diferenciaPrioridad = (b.prioridad?.nivel ?? 0) - (a.prioridad?.nivel ?? 0);
    if (diferenciaPrioridad !== 0) return diferenciaPrioridad;
    if (!a.fechaCompromiso) return 1;
    if (!b.fechaCompromiso) return -1;
    return a.fechaCompromiso.localeCompare(b.fechaCompromiso);
  });
}

/**
 * Analista: cola de trabajo, no una tabla genérica ni un Kanban. Dos grupos que nunca
 * se mezclan y nunca comparten la misma acción (ADR-0012: asignadas a sí mismo + sin responsable).
 */
export function AnalistaQueueView() {
  const usuario = useAuthStore((state) => state.user);
  const [grupo, setGrupo] = useState<Grupo>('asignadas');
  const [valoresFiltros, setValoresFiltros] = useState<ValoresFiltrosSolicitudes>(FILTROS_VACIOS);
  const filtros = useMemo(() => convertirAFiltrosSolicitudes(valoresFiltros), [valoresFiltros]);

  const { data: asignadas = [] } = useSolicitudesAsignadas(usuario?.id ?? 0, filtros);
  const { data: disponibles = [] } = useSolicitudesDisponibles(filtros);

  const asignadasOrdenadas = useMemo(() => ordenarPorUrgencia(asignadas), [asignadas]);
  const disponiblesOrdenadas = useMemo(() => ordenarPorUrgencia(disponibles), [disponibles]);

  const filas = grupo === 'asignadas' ? asignadasOrdenadas : disponiblesOrdenadas;

  return (
    <div className="flex flex-col gap-6">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <SegmentedControl
          valor={grupo}
          onChange={(valor) => setGrupo(valor as Grupo)}
          opciones={[
            { valor: 'asignadas', etiqueta: 'Asignadas a mí', contador: asignadasOrdenadas.length },
            { valor: 'disponibles', etiqueta: 'Disponibles para tomar', contador: disponiblesOrdenadas.length },
          ]}
        />
        {/* Sin filtro de Solicitante/Responsable: es implícitamente el propio analista. */}
        <SolicitudFilters campos={['estado', 'prioridad']} valores={valoresFiltros} onChange={setValoresFiltros} />
      </div>

      {filas.length === 0 ? (
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
        <ul className="flex flex-col gap-2">
          {filas.map((solicitud) => (
            <SolicitudQueueItem
              key={solicitud.id}
              solicitud={solicitud}
              accion={
                grupo === 'asignadas' ? (
                  <button type="button" className={buttonClasses('secundario', 'sm')}>
                    Cambiar estado
                  </button>
                ) : (
                  <button type="button" className={buttonClasses('primario', 'sm')}>
                    Tomar
                  </button>
                )
              }
            />
          ))}
        </ul>
      )}
    </div>
  );
}
