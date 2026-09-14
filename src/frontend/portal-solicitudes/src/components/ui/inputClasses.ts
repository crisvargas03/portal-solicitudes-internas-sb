// Clase canónica repetida hoy en Login, SolicitudForm, SolicitudFilters y FilterSelect —
// centralizada aquí para que el estado de error se agregue en un solo lugar.
const BASE = 'rounded-md border px-3 py-2 text-sm outline-none focus:border-accent-orange';
const SIN_ERROR = 'border-slate-300';
const CON_ERROR = 'border-danger ring-1 ring-danger focus:border-danger';

/** Clases para un input/select/textarea, según tenga error de validación o no. */
export function inputClasses(conError: boolean): string {
  return `${BASE} ${conError ? CON_ERROR : SIN_ERROR}`;
}
