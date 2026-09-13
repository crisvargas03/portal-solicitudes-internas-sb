interface VencimientoIndicatorProps {
  fechaCompromiso: string | null;
  estaVencida: boolean;
}

function diferenciaEnDias(fechaCompromiso: string): number {
  const hoy = new Date();
  hoy.setHours(0, 0, 0, 0);
  const fecha = new Date(`${fechaCompromiso}T00:00:00`);
  return Math.round((fecha.getTime() - hoy.getTime()) / (1000 * 60 * 60 * 24));
}

/** Usa siempre el estaVencida del servidor; los días mostrados son solo texto derivado de la fecha. */
export function VencimientoIndicator({ fechaCompromiso, estaVencida }: VencimientoIndicatorProps) {
  if (!fechaCompromiso) {
    return <span className="text-xs text-slate-400">Sin vencimiento</span>;
  }

  const dias = diferenciaEnDias(fechaCompromiso);
  const clase = estaVencida ? 'text-red-600 font-semibold' : dias === 0 ? 'text-accent-orange font-semibold' : 'text-slate-500';

  let texto: string;
  if (dias < 0) {
    texto = `Vencida hace ${Math.abs(dias)} día${Math.abs(dias) === 1 ? '' : 's'}`;
  } else if (dias === 0) {
    texto = 'Vence hoy';
  } else {
    texto = `Vence en ${dias} día${dias === 1 ? '' : 's'}`;
  }

  return <span className={`text-xs ${clase}`}>{texto}</span>;
}
