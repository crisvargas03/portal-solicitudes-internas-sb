import { USUARIOS } from '../../mocks/catalogos';
import { useAreas, useEstadosSolicitud, usePrioridades, useTiposSolicitud } from '../../hooks/queries/useCatalogos';
import { FilterSelect } from '../ui/FilterSelect';
import type { CampoFiltro, ValoresFiltrosSolicitudes } from './filtrosSolicitudes';

interface SolicitudFiltersProps {
  campos: CampoFiltro[];
  valores: ValoresFiltrosSolicitudes;
  onChange: (valores: ValoresFiltrosSolicitudes) => void;
}

/** Un único componente de filtros, configurado por rol vía `campos` — evita que las reglas diverjan entre vistas. */
export function SolicitudFilters({ campos, valores, onChange }: SolicitudFiltersProps) {
  function actualizar<K extends keyof ValoresFiltrosSolicitudes>(campo: K, valor: string) {
    onChange({ ...valores, [campo]: valor });
  }

  const { data: areas = [] } = useAreas();
  const { data: tiposSolicitud = [] } = useTiposSolicitud();
  const { data: prioridades = [] } = usePrioridades();
  const { data: estadosSolicitud = [] } = useEstadosSolicitud();

  // Usuarios (solicitantes/analistas para los filtros de "Solicitante"/"Responsable") sigue
  // en mocks: no hay un endpoint de catálogo de usuarios en el alcance de este cambio.
  const analistas = USUARIOS.filter((usuario) => usuario.rol === 'Analista');
  const solicitantes = USUARIOS.filter((usuario) => usuario.rol === 'Solicitante');

  return (
    <div className="flex flex-wrap items-center gap-2">
      {campos.includes('estado') && (
        <FilterSelect
          etiqueta="Estado"
          valor={valores.estadoCodigo}
          onChange={(valor) => actualizar('estadoCodigo', valor)}
          opciones={estadosSolicitud.map((estado) => ({ valor: estado.codigo, etiqueta: estado.nombre }))}
        />
      )}
      {campos.includes('prioridad') && (
        <FilterSelect
          etiqueta="Prioridad"
          valor={valores.prioridadId}
          onChange={(valor) => actualizar('prioridadId', valor)}
          opciones={prioridades.map((prioridad) => ({ valor: String(prioridad.id), etiqueta: prioridad.nombre }))}
        />
      )}
      {campos.includes('area') && (
        <FilterSelect
          etiqueta="Área"
          valor={valores.areaId}
          onChange={(valor) => actualizar('areaId', valor)}
          opciones={areas.map((area) => ({ valor: String(area.id), etiqueta: area.nombre }))}
        />
      )}
      {campos.includes('tipo') && (
        <FilterSelect
          etiqueta="Tipo"
          valor={valores.tipoSolicitudId}
          onChange={(valor) => actualizar('tipoSolicitudId', valor)}
          opciones={tiposSolicitud.map((tipo) => ({ valor: String(tipo.id), etiqueta: tipo.nombre }))}
        />
      )}
      {campos.includes('solicitante') && (
        <FilterSelect
          etiqueta="Solicitante"
          valor={valores.usuarioSolicitanteId}
          onChange={(valor) => actualizar('usuarioSolicitanteId', valor)}
          opciones={solicitantes.map((usuario) => ({ valor: String(usuario.id), etiqueta: usuario.nombre }))}
        />
      )}
      {campos.includes('responsable') && (
        <FilterSelect
          etiqueta="Responsable"
          valor={valores.usuarioAsignadoId}
          onChange={(valor) => actualizar('usuarioAsignadoId', valor)}
          opciones={analistas.map((usuario) => ({ valor: String(usuario.id), etiqueta: usuario.nombre }))}
        />
      )}
      {campos.includes('rangoFecha') && (
        <div className="flex items-center gap-1.5">
          <input
            type="date"
            value={valores.fechaDesde}
            onChange={(evento) => actualizar('fechaDesde', evento.target.value)}
            className="rounded-md border border-slate-300 px-2.5 py-1.5 text-sm text-slate-600 outline-none focus:border-accent-orange"
          />
          <span className="text-xs text-slate-400">a</span>
          <input
            type="date"
            value={valores.fechaHasta}
            onChange={(evento) => actualizar('fechaHasta', evento.target.value)}
            className="rounded-md border border-slate-300 px-2.5 py-1.5 text-sm text-slate-600 outline-none focus:border-accent-orange"
          />
        </div>
      )}
    </div>
  );
}
