import { RoleSwitch } from '../components/common/RoleSwitch';
import { AdminDashboardView } from '../components/dashboard/views/AdminDashboardView';
import { AnalistaDashboardView } from '../components/dashboard/views/AnalistaDashboardView';
import { SolicitanteDashboardView } from '../components/dashboard/views/SolicitanteDashboardView';

export function Dashboard() {
  return (
    <RoleSwitch
      administrador={<AdminDashboardView />}
      analista={<AnalistaDashboardView />}
      solicitante={<SolicitanteDashboardView />}
    />
  );
}
