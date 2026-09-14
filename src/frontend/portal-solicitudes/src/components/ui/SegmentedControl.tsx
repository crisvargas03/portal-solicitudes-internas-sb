interface OpcionSegmentada {
  valor: string;
  etiqueta: string;
  contador?: number;
}

interface SegmentedControlProps {
  opciones: OpcionSegmentada[];
  valor: string;
  onChange: (valor: string) => void;
}

/** Pill toggle — usado por la cola de Analista para alternar entre grupos, nunca mezclarlos. */
export function SegmentedControl({ opciones, valor, onChange }: SegmentedControlProps) {
  return (
    <div className="inline-flex rounded-md border border-slate-200 bg-white p-1">
      {opciones.map((opcion) => {
        const activo = opcion.valor === valor;
        return (
          <button
            key={opcion.valor}
            type="button"
            onClick={() => onChange(opcion.valor)}
            className={`flex items-center gap-1.5 rounded px-3 py-1.5 text-sm font-medium transition-colors ${
              activo ? 'bg-navy text-white' : 'text-slate-500 hover:text-navy'
            }`}
          >
            {opcion.etiqueta}
            {opcion.contador !== undefined && (
              <span className={`rounded-full px-1.5 text-xs ${activo ? 'bg-white/20' : 'bg-slate-100'}`}>{opcion.contador}</span>
            )}
          </button>
        );
      })}
    </div>
  );
}
