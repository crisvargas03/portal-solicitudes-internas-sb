import { AlertTriangle, CheckCircle2, Clock3, ListChecks } from 'lucide-react';
import type { LucideIcon } from 'lucide-react';
import type { Tono } from '../ui/tono';

interface MetricaConfig {
  icono: LucideIcon;
  tono: Tono;
}

/** Icono + color por tipo de métrica, en un único lugar — así "vencidas" siempre es naranja, "abiertas" siempre navy. */
export const METRICAS_CONFIG = {
  abiertas: { icono: ListChecks, tono: 'navy' },
  cerradas: { icono: CheckCircle2, tono: 'exito' },
  vencidas: { icono: AlertTriangle, tono: 'acento' },
  recientes: { icono: Clock3, tono: 'info' },
} satisfies Record<string, MetricaConfig>;
