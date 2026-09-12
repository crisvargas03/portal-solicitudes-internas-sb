import { ArrowLeft } from 'lucide-react';
import { Link, useParams } from 'react-router';

const MOCK_HISTORIAL = [
  { id: 1, estado: 'Registrada', usuario: 'María Peña', fecha: '2026-09-01' },
  { id: 2, estado: 'En análisis', usuario: 'Carlos Díaz', fecha: '2026-09-03' },
];

const MOCK_COMENTARIOS = [
  { id: 1, usuario: 'Carlos Díaz', texto: 'Se solicitó información adicional al usuario.', fecha: '2026-09-03' },
];

export function SolicitudDetail() {
  const { id } = useParams();

  return (
    <div className="flex flex-col gap-6">
      <Link to="/solicitudes" className="flex items-center gap-1.5 text-sm text-slate-500 hover:text-accent-orange">
        <ArrowLeft size={16} />
        Volver a solicitudes
      </Link>

      <div>
        <p className="text-sm text-slate-500">SOL-2026-{id?.padStart(4, '0')}</p>
        <h2 className="text-xl font-semibold text-navy">Acceso a sistema de reportes</h2>
        <p className="mt-2 max-w-2xl text-sm text-slate-600">
          Descripción de la solicitud pendiente de datos reales del backend.
        </p>
      </div>

      <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
        <Field label="Estado" value="En análisis" />
        <Field label="Prioridad" value="Alta" />
        <Field label="Área" value="Tecnología" />
        <Field label="Responsable" value="Carlos Díaz" />
      </div>

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
