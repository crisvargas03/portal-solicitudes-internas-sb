type Variante = 'primario' | 'secundario' | 'fantasma';
type Tamano = 'sm' | 'md';

const VARIANTE_CLASES: Record<Variante, string> = {
  primario: 'bg-navy text-white hover:bg-accent-orange',
  secundario: 'border border-slate-300 text-navy hover:border-accent-orange hover:text-accent-orange',
  fantasma: 'text-slate-500 hover:text-accent-orange',
};

const TAMANO_CLASES: Record<Tamano, string> = {
  sm: 'px-3 py-1.5 text-xs',
  md: 'px-4 py-2 text-sm',
};

/** Clases compartidas por <Button> y por cualquier <Link> que deba verse como botón. */
export function buttonClasses(variante: Variante = 'primario', tamano: Tamano = 'md'): string {
  return `inline-flex items-center justify-center gap-2 rounded-md font-medium transition-colors ${VARIANTE_CLASES[variante]} ${TAMANO_CLASES[tamano]}`;
}
