import { useState } from 'react';
import { ArrowLeft, FileSearch, Pencil, RefreshCw } from 'lucide-react';
import { Link, useParams } from 'react-router';
import { CambiarEstadoModal } from '../components/solicitudes/CambiarEstadoModal';
import { ComentariosSection } from '../components/solicitudes/ComentariosSection';
import { EvidenciaSection } from '../components/solicitudes/EvidenciaSection';
import { HistorialTimeline } from '../components/solicitudes/HistorialTimeline';
import { SolicitudInfoPanel } from '../components/solicitudes/SolicitudInfoPanel';
import { Button } from '../components/ui/Button';
import { Card } from '../components/ui/Card';
import { EmptyState } from '../components/ui/EmptyState';
import { useSolicitud, useTransiciones } from '../hooks/queries/useSolicitudes';
import { useRolActual } from '../hooks/useRolActual';

export function SolicitudDetail() {
  const { id } = useParams();
  const idNumerico = Number(id);
  const { esAdministrador, esAnalista } = useRolActual();
  const { data: solicitud, isLoading, isError } = useSolicitud(idNumerico);
  const { data: transiciones = [] } = useTransiciones(idNumerico);
  const [modalEstadoAbierto, setModalEstadoAbierto] = useState(false);

  if (isLoading) {
    return <p className="text-sm text-slate-500">Cargando…</p>;
  }

  if (isError || !solicitud) {
    return (
      <EmptyState
        icono={FileSearch}
        titulo="La solicitud no existe"
        descripcion="Verifique el enlace o vuelva al listado de solicitudes."
        accion={
          <Link to="/solicitudes" className="text-sm text-accent-orange hover:underline">
            Volver a solicitudes
          </Link>
        }
      />
    );
  }

  return (
    <div className="flex flex-col gap-6">
      <div className="flex items-center justify-between">
        <Link to="/solicitudes" className="flex items-center gap-1.5 text-sm text-slate-500 hover:text-accent-orange">
          <ArrowLeft size={16} />
          Volver a solicitudes
        </Link>
        <div className="flex items-center gap-3">
          {esAdministrador && (
            <Link to={`/solicitudes/${id}/editar`} className="flex items-center gap-1.5 text-sm text-accent-orange hover:underline">
              <Pencil size={14} />
              Editar solicitud
            </Link>
          )}
          {transiciones.length > 0 && (
            <Button type="button" variante="secundario" tamano="sm" icono={RefreshCw} onClick={() => setModalEstadoAbierto(true)}>
              Cambiar estado
            </Button>
          )}
        </div>
      </div>

      <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
        <div className="flex flex-col gap-6 lg:col-span-2">
          <div>
            <p className="text-sm text-slate-500">{solicitud.codigo}</p>
            <h2 className="text-xl font-semibold text-navy">{solicitud.titulo}</h2>
            <p className="mt-2 text-sm text-slate-600">{solicitud.descripcion}</p>
          </div>

          <EvidenciaSection solicitudId={idNumerico} adjuntos={solicitud.adjuntos} />

          <ComentariosSection
            solicitudId={idNumerico}
            comentarios={solicitud.comentarios}
            puedeComentarInterno={esAdministrador || esAnalista}
          />
        </div>

        <SolicitudInfoPanel solicitud={solicitud} puedeReasignar={esAdministrador} />
      </div>

      <Card titulo="Historial">
        <HistorialTimeline historial={solicitud.historial} />
      </Card>

      {modalEstadoAbierto && <CambiarEstadoModal solicitudId={idNumerico} onCerrar={() => setModalEstadoAbierto(false)} />}
    </div>
  );
}
