import { useParams } from 'react-router';
import { useAreas, usePrioridades, useTiposSolicitud } from '../hooks/queries/useCatalogos';

export function SolicitudForm() {
  const { id } = useParams();
  const isEdit = Boolean(id);

  const { data: tiposSolicitud = [] } = useTiposSolicitud();
  const { data: areas = [] } = useAreas();
  const { data: prioridades = [] } = usePrioridades();

  return (
    <div className="flex flex-col gap-6">
      <h2 className="text-xl font-semibold text-navy">
        {isEdit ? 'Editar solicitud' : 'Nueva solicitud'}
      </h2>

      <form className="grid max-w-2xl grid-cols-1 gap-4 sm:grid-cols-2">
        <label className="flex flex-col gap-1.5 text-sm font-medium text-slate-700 sm:col-span-2">
          Título
          <input
            type="text"
            className="rounded-md border border-slate-300 px-3 py-2 text-sm outline-none focus:border-accent-orange"
            placeholder="Ej: Acceso a sistema de reportes"
          />
        </label>

        <label className="flex flex-col gap-1.5 text-sm font-medium text-slate-700 sm:col-span-2">
          Descripción
          <textarea
            rows={4}
            className="rounded-md border border-slate-300 px-3 py-2 text-sm outline-none focus:border-accent-orange"
            placeholder="Detalle de la solicitud"
          />
        </label>

        <label className="flex flex-col gap-1.5 text-sm font-medium text-slate-700">
          Tipo de solicitud
          <select className="rounded-md border border-slate-300 px-3 py-2 text-sm outline-none focus:border-accent-orange">
            {tiposSolicitud.map((tipo) => (
              <option key={tipo.id} value={tipo.id}>
                {tipo.nombre}
              </option>
            ))}
          </select>
        </label>

        <label className="flex flex-col gap-1.5 text-sm font-medium text-slate-700">
          Área
          <select className="rounded-md border border-slate-300 px-3 py-2 text-sm outline-none focus:border-accent-orange">
            {areas.map((area) => (
              <option key={area.id} value={area.id}>
                {area.nombre}
              </option>
            ))}
          </select>
        </label>

        <label className="flex flex-col gap-1.5 text-sm font-medium text-slate-700">
          Prioridad
          <select className="rounded-md border border-slate-300 px-3 py-2 text-sm outline-none focus:border-accent-orange">
            {prioridades.map((prioridad) => (
              <option key={prioridad.id} value={prioridad.id}>
                {prioridad.nombre}
              </option>
            ))}
          </select>
        </label>

        <label className="flex flex-col gap-1.5 text-sm font-medium text-slate-700">
          Fecha compromiso
          <input
            type="date"
            className="rounded-md border border-slate-300 px-3 py-2 text-sm outline-none focus:border-accent-orange"
          />
        </label>

        <button
          type="submit"
          className="mt-2 w-fit rounded-md bg-navy px-5 py-2.5 text-sm font-medium text-white transition-colors hover:bg-accent-orange sm:col-span-2"
        >
          {isEdit ? 'Guardar cambios' : 'Registrar solicitud'}
        </button>
      </form>
    </div>
  );
}
