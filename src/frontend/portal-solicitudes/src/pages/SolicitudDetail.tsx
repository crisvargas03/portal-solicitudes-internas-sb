import { useState } from 'react';
import { ArrowLeft, Pencil } from 'lucide-react';
import { Link, useParams } from 'react-router';
import { PriorityBadge } from '../components/solicitudes/PriorityBadge';
import { StatusBadge } from '../components/solicitudes/StatusBadge';
import { Button } from '../components/ui/Button';
import { inputClasses } from '../components/ui/inputClasses';
import { useAnalistas } from '../hooks/queries/useUsuarios';
import { useCambiarAsignacion } from '../hooks/queries/useSolicitudes';
import { useRolActual } from '../hooks/useRolActual';
import { ErrorApi } from '../lib/apiClient';
import { PRIORIDADES, obtenerEstadoPorCodigo } from '../mocks/catalogos';
import { CodigosEstadoSolicitud } from '../types';

// Pendiente de datos reales del backend — estado/prioridad de ejemplo solo para
// mostrar los badges compartidos con la lista y el dashboard.
const ESTADO_MOCK = obtenerEstadoPorCodigo(CodigosEstadoSolicitud.EN_ANALISIS);
const PRIORIDAD_MOCK = PRIORIDADES.find((prioridad) => prioridad.nivel === 3);

const MOCK_HISTORIAL = [
  { id: 1, estado: 'Registrada', usuario: 'María Peña', fecha: '2026-09-01' },
  { id: 2, estado: 'En análisis', usuario: 'Carlos Díaz', fecha: '2026-09-03' },
];

const MOCK_COMENTARIOS = [
  { id: 1, usuario: 'Carlos Díaz', texto: 'Se solicitó información adicional al usuario.', fecha: '2026-09-03' },
];

export function SolicitudDetail() {
  const { id } = useParams();
  const idNumerico = Number(id);
  const { esAdministrador } = useRolActual();
  const { data: analistas = [] } = useAnalistas();
  const cambiarAsignacion = useCambiarAsignacion();
  const [analistaSeleccionado, setAnalistaSeleccionado] = useState('');
  const [errorAsignacion, setErrorAsignacion] = useState<string | null>(null);

  async function asignar() {
    setErrorAsignacion(null);
    try {
      await cambiarAsignacion.mutateAsync({
        id: idNumerico,
        usuarioAsignadoId: analistaSeleccionado ? Number(analistaSeleccionado) : null,
      });
    } catch (error) {
      setErrorAsignacion(error instanceof ErrorApi ? error.message : 'No se pudo asignar la solicitud.');
    }
  }

  return (
    <div className="flex flex-col gap-6">
      <div className="flex items-center justify-between">
        <Link to="/solicitudes" className="flex items-center gap-1.5 text-sm text-slate-500 hover:text-accent-orange">
          <ArrowLeft size={16} />
          Volver a solicitudes
        </Link>
        {esAdministrador && (
          <Link to={`/solicitudes/${id}/editar`} className="flex items-center gap-1.5 text-sm text-accent-orange hover:underline">
            <Pencil size={14} />
            Editar solicitud
          </Link>
        )}
      </div>

      <div>
        <p className="text-sm text-slate-500">SOL-2026-{id?.padStart(4, '0')}</p>
        <h2 className="text-xl font-semibold text-navy">Acceso a sistema de reportes</h2>
        <p className="mt-2 max-w-2xl text-sm text-slate-600">
          Descripción de la solicitud pendiente de datos reales del backend.
        </p>
      </div>

      <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
        <div>
          <p className="text-xs text-slate-500">Estado</p>
          <div className="mt-1">
            <StatusBadge estado={ESTADO_MOCK} />
          </div>
        </div>
        <div>
          <p className="text-xs text-slate-500">Prioridad</p>
          <div className="mt-1">
            <PriorityBadge prioridad={PRIORIDAD_MOCK} />
          </div>
        </div>
        <Field label="Área" value="Tecnología" />
        <Field label="Responsable" value="Carlos Díaz" />
      </div>

      {esAdministrador && (
        <section className="max-w-md rounded-md border border-slate-200 p-4">
          <h3 className="mb-3 text-sm font-semibold text-navy">Asignar responsable</h3>
          {errorAsignacion && <p className="mb-3 rounded-md bg-danger-soft px-3 py-2 text-xs text-danger">{errorAsignacion}</p>}
          <div className="flex items-end gap-3">
            <label className="flex flex-1 flex-col gap-1.5 text-sm font-medium text-slate-700">
              Analista
              <select
                className={inputClasses(false)}
                value={analistaSeleccionado}
                onChange={(event) => setAnalistaSeleccionado(event.target.value)}
              >
                <option value="">Sin asignar</option>
                {analistas.map((analista) => (
                  <option key={analista.id} value={analista.id}>
                    {analista.nombre}
                  </option>
                ))}
              </select>
            </label>
            <Button type="button" cargando={cambiarAsignacion.isPending} onClick={asignar}>
              Asignar
            </Button>
          </div>
        </section>
      )}

      <section>
        <h3 className="mb-3 text-sm font-semibold text-navy">Historial de estados</h3>
        <ul className="flex flex-col divide-y divide-slate-200 rounded-md border border-slate-200">
          {MOCK_HISTORIAL.map((entry) => (
            <li key={entry.id} className="flex items-center justify-between px-4 py-3 text-sm">
              <span className="font-medium text-navy">{entry.estado}</span>
              <span className="text-slate-500">
                {entry.usuario} · {entry.fecha}
              </span>
            </li>
          ))}
        </ul>
      </section>

      <section>
        <h3 className="mb-3 text-sm font-semibold text-navy">Comentarios</h3>
        <ul className="flex flex-col gap-3">
          {MOCK_COMENTARIOS.map((comentario) => (
            <li key={comentario.id} className="rounded-md border border-slate-200 px-4 py-3 text-sm">
              <p className="text-slate-700">{comentario.texto}</p>
              <p className="mt-1 text-xs text-slate-500">
                {comentario.usuario} · {comentario.fecha}
              </p>
            </li>
          ))}
        </ul>
      </section>
    </div>
  );
}

function Field({ label, value }: { label: string; value: string }) {
  return (
    <div>
      <p className="text-xs text-slate-500">{label}</p>
      <p className="text-sm font-medium text-navy">{value}</p>
    </div>
  );
}
