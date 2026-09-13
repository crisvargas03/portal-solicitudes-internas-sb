interface OpcionFiltro {
  valor: string;
  etiqueta: string;
}

interface FilterSelectProps {
  etiqueta: string;
  opciones: OpcionFiltro[];
  valor: string;
  onChange: (valor: string) => void;
  permiteTodos?: boolean;
}

export function FilterSelect({ etiqueta, opciones, valor, onChange, permiteTodos = true }: FilterSelectProps) {
  return (
    <select
      value={valor}
      onChange={(event) => onChange(event.target.value)}
      className="rounded-md border border-slate-300 px-3 py-1.5 text-sm text-slate-600 outline-none focus:border-accent-orange"
    >
      {permiteTodos && <option value="">{etiqueta}</option>}
      {opciones.map((opcion) => (
        <option key={opcion.valor} value={opcion.valor}>
          {opcion.etiqueta}
        </option>
      ))}
    </select>
  );
}
