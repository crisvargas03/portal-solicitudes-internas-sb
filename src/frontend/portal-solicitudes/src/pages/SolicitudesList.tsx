import { Plus } from 'lucide-react';
import { Link } from 'react-router';

const MOCK_ROWS = [
  { id: 1, codigo: 'SOL-2026-0031', titulo: 'Acceso a sistema de reportes', estado: 'En análisis', prioridad: 'Alta', area: 'Tecnología', solicitante: 'María Peña' },
  { id: 2, codigo: 'SOL-2026-0030', titulo: 'Renovación de licencia', estado: 'Registrada', prioridad: 'Media', area: 'Legal', solicitante: 'Juan Cruz' },
  { id: 3, codigo: 'SOL-2026-0029', titulo: 'Solicitud de equipo', estado: 'En progreso', prioridad: 'Baja', area: 'Recursos Humanos', solicitante: 'Ana Reyes' },
];

const FILTROS = ['Estado', 'Prioridad', 'Área', 'Solicitante', 'Responsable', 'Rango de fecha'];

export function SolicitudesList() {
  return (
    <div className="flex flex-col gap-6">
      <div className="flex items-center justify-between">
        <div className="flex flex-wrap gap-2">
          {FILTROS.map((filtro) => (
            <select
              key={filtro}
              defaultValue=""
              className="rounded-md border border-slate-300 px-3 py-1.5 text-sm text-slate-600 outline-none focus:border-accent-orange"
            >
              <option value="" disabled>
                {filtro}
              </option>
            </select>
          ))}
        </div>
        <Link
          to="/solicitudes/nueva"
          className="flex items-center gap-2 rounded-md bg-navy px-4 py-2 text-sm font-medium text-white transition-colors hover:bg-accent-orange"
        >
          <Plus size={16} />
          Nueva solicitud
        </Link>
      </div>

      <div className="overflow-x-auto rounded-md border border-slate-200">
        <table className="w-full text-left text-sm">
          <thead className="bg-gray-bg text-xs font-semibold uppercase tracking-wide text-slate-500">
            <tr>
              <th className="px-4 py-3">Código</th>
              <th className="px-4 py-3">Título</th>
              <th className="px-4 py-3">Estado</th>
              <th className="px-4 py-3">Prioridad</th>
              <th className="px-4 py-3">Área</th>
              <th className="px-4 py-3">Solicitante</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-200">
            {MOCK_ROWS.map((row) => (
              <tr key={row.id} className="hover:bg-gray-bg/60">
                <td className="px-4 py-3">
                  <Link to={`/solicitudes/${row.id}`} className="font-medium text-accent-orange hover:underline">
                    {row.codigo}
                  </Link>
                </td>
                <td className="px-4 py-3 text-navy">{row.titulo}</td>
                <td className="px-4 py-3 text-slate-600">{row.estado}</td>
                <td className="px-4 py-3 text-slate-600">{row.prioridad}</td>
                <td className="px-4 py-3 text-slate-600">{row.area}</td>
                <td className="px-4 py-3 text-slate-600">{row.solicitante}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
