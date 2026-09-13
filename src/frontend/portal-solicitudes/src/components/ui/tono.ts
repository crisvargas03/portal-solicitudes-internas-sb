/**
 * Paleta semántica compartida por todos los primitivos de /ui.
 * Cualquier componente que necesite colorear por significado (estado, prioridad,
 * tipo de métrica) debe usar un Tono en lugar de una clase de Tailwind suelta.
 */
export type Tono = 'neutral' | 'oscuro' | 'info' | 'navy' | 'exito' | 'advertencia' | 'acento' | 'critico';

interface ClasesTono {
  /** Fondo tenue + texto de contraste, para badges y puntos de lista. */
  suave: string;
  /** Fondo sólido + texto blanco, reservado para señales que deben interrumpir la lectura. */
  solido: string;
  /** Un único color plano, para puntos indicadores y barras. */
  punto: string;
}

export const TONO_CLASES: Record<Tono, ClasesTono> = {
  neutral: { suave: 'bg-slate-100 text-slate-600', solido: 'bg-slate-500 text-white', punto: 'bg-slate-400' },
  oscuro: { suave: 'bg-slate-200 text-slate-700', solido: 'bg-slate-700 text-white', punto: 'bg-slate-600' },
  info: { suave: 'bg-sky-100 text-sky-800', solido: 'bg-sky-700 text-white', punto: 'bg-sky-500' },
  navy: { suave: 'bg-navy/10 text-navy', solido: 'bg-navy text-white', punto: 'bg-navy' },
  exito: { suave: 'bg-emerald-100 text-emerald-800', solido: 'bg-emerald-700 text-white', punto: 'bg-emerald-500' },
  advertencia: { suave: 'bg-amber-100 text-amber-800', solido: 'bg-amber-600 text-white', punto: 'bg-amber-500' },
  acento: { suave: 'bg-accent-orange/10 text-accent-orange', solido: 'bg-accent-orange text-white', punto: 'bg-accent-orange' },
  critico: { suave: 'bg-red-100 text-red-800', solido: 'bg-red-700 text-white', punto: 'bg-red-600' },
};
