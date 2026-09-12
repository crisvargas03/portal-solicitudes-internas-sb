import { AlertTriangle, Clock3, ListChecks, Signal } from 'lucide-react';

const METRICAS_ESTADO = [
  { label: 'Registrada', valor: 8 },
  { label: 'En análisis', valor: 5 },
  { label: 'En progreso', valor: 12 },
  { label: 'Resuelta', valor: 21 },
];

const METRICAS_PRIORIDAD = [
  { label: 'Alta', valor: 6 },
  { label: 'Media', valor: 15 },
  { label: 'Baja', valor: 9 },
];

const ULTIMAS_SOLICITUDES = [
  { codigo: 'SOL-2026-0031', titulo: 'Acceso a sistema de reportes', area: 'Tecnología' },
  { codigo: 'SOL-2026-0030', titulo: 'Renovación de licencia', area: 'Legal' },
  { codigo: 'SOL-2026-0029', titulo: 'Solicitud de equipo', area: 'Recursos Humanos' },
];

export function Dashboard() {
  return (
    <div className="flex flex-col gap-8">
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <MetricCard icon={ListChecks} label="Total por estado" valor="46" />
        <MetricCard icon={Signal} label="Total por prioridad" valor="30" />
        <MetricCard icon={AlertTriangle} label="Vencidas" valor="4" />
        <MetricCard icon={Clock3} label="Últimas 7 días" valor="9" />
      </div>

      <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
        <section>
          <h2 className="mb-3 text-sm font-semibold text-navy">Por estado</h2>
          <ul className="flex flex-col gap-2">
            {METRICAS_ESTADO.map((item) => (
              <li
                key={item.label}
                className="flex items-center justify-between rounded-md border border-slate-200 px-4 py-2.5 text-sm text-slate-700"
              >
                {item.label}
                <span className="font-semibold text-navy">{item.valor}</span>
              </li>
            ))}
          </ul>
        </section>

        <section>
          <h2 className="mb-3 text-sm font-semibold text-navy">Por prioridad</h2>
          <ul className="flex flex-col gap-2">
            {METRICAS_PRIORIDAD.map((item) => (
              <li
                key={item.label}
                className="flex items-center justify-between rounded-md border border-slate-200 px-4 py-2.5 text-sm text-slate-700"
              >
                {item.label}
                <span className="font-semibold text-navy">{item.valor}</span>
              </li>
            ))}
          </ul>
        </section>
      </div>

      <section>
        <h2 className="mb-3 text-sm font-semibold text-navy">Últimas registradas</h2>
        <ul className="flex flex-col divide-y divide-slate-200 rounded-md border border-slate-200">
          {ULTIMAS_SOLICITUDES.map((solicitud) => (
            <li key={solicitud.codigo} className="flex items-center justify-between px-4 py-3 text-sm">
              <div>
                <p className="font-medium text-navy">{solicitud.titulo}</p>
                <p className="text-slate-500">{solicitud.codigo}</p>
              </div>
              <span className="text-slate-500">{solicitud.area}</span>
            </li>
          ))}
        </ul>
      </section>
    </div>
  );
}

interface MetricCardProps {
  icon: typeof ListChecks;
  label: string;
  valor: string;
}

function MetricCard({ icon: Icon, label, valor }: MetricCardProps) {
  return (
    <div className="flex items-center gap-4 rounded-md border border-slate-200 p-4">
      <Icon className="text-accent-orange" size={22} />
      <div>
        <p className="text-xs text-slate-500">{label}</p>
        <p className="text-xl font-semibold text-navy">{valor}</p>
      </div>
    </div>
  );
}
