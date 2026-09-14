import { useState } from 'react';
import { AreasAdminSection } from '../components/catalogos/AreasAdminSection';
import { PrioridadesAdminSection } from '../components/catalogos/PrioridadesAdminSection';
import { TiposSolicitudAdminSection } from '../components/catalogos/TiposSolicitudAdminSection';
import { SegmentedControl } from '../components/ui/SegmentedControl';

type CatalogoActivo = 'areas' | 'prioridades' | 'tiposSolicitud';

const OPCIONES = [
  { valor: 'areas', etiqueta: 'Áreas' },
  { valor: 'prioridades', etiqueta: 'Prioridades' },
  { valor: 'tiposSolicitud', etiqueta: 'Tipos de solicitud' },
];

// Estados no tiene sección aquí: es un flujo fijo de 6 estados sin CRUD (ver ADR-0020).
export function AdminCatalogos() {
  const [catalogo, setCatalogo] = useState<CatalogoActivo>('areas');

  return (
    <div className="flex flex-col gap-6">
      <div>
        <h2 className="text-xl font-semibold text-navy">Catálogos</h2>
        <p className="text-sm text-slate-500">Áreas, prioridades y tipos de solicitud administrables.</p>
      </div>

      <SegmentedControl opciones={OPCIONES} valor={catalogo} onChange={(valor) => setCatalogo(valor as CatalogoActivo)} />

      {catalogo === 'areas' && <AreasAdminSection />}
      {catalogo === 'prioridades' && <PrioridadesAdminSection />}
      {catalogo === 'tiposSolicitud' && <TiposSolicitudAdminSection />}
    </div>
  );
}
