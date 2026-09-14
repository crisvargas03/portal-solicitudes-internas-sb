import { EntidadesGubernamentalesAdminSection } from '../components/catalogos/EntidadesGubernamentalesAdminSection';

// Página propia en el sidebar, no un tab de Admin → Catálogos: a diferencia de Áreas/
// Prioridades/TiposSolicitud (catálogos chicos del dominio de Solicitudes), este es un
// catálogo grande y autónomo (~180 registros, respaldado por archivo, ver ADR-0034).
export function AdminEntidadesGubernamentales() {
  return (
    <div className="flex flex-col gap-6">
      <div>
        <h2 className="text-xl font-semibold text-navy">Entidades gubernamentales</h2>
        <p className="text-sm text-slate-500">Catálogo de entidades del Estado dominicano usado como referencia.</p>
      </div>

      <EntidadesGubernamentalesAdminSection />
    </div>
  );
}
