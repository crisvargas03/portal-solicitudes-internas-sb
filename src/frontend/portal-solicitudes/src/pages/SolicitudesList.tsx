import { RoleSwitch } from '../components/common/RoleSwitch';
import { AdminSolicitudesView } from '../components/solicitudes/views/AdminSolicitudesView';
import { AnalistaQueueView } from '../components/solicitudes/views/AnalistaQueueView';
import { SolicitanteSolicitudesView } from '../components/solicitudes/views/SolicitanteSolicitudesView';

export function SolicitudesList() {
  return (
    <RoleSwitch
      administrador={<AdminSolicitudesView />}
      analista={<AnalistaQueueView />}
      solicitante={<SolicitanteSolicitudesView />}
    />
  );
}
