import { createBrowserRouter, Navigate } from 'react-router';
import { RutaPorRol } from '../components/common/RutaPorRol';
import { RutaProtegida } from '../components/common/RutaProtegida';
import { AppLayout } from '../components/layout/AppLayout';
import { Dashboard } from '../pages/Dashboard';
import { Login } from '../pages/Login';
import { SolicitudDetail } from '../pages/SolicitudDetail';
import { SolicitudesList } from '../pages/SolicitudesList';
import { SolicitudForm } from '../pages/SolicitudForm';

export const router = createBrowserRouter([
	{ path: '/login', element: <Login /> },
	{
		path: '/',
		element: (
			<RutaProtegida>
				<AppLayout />
			</RutaProtegida>
		),
		children: [
			{ index: true, element: <Navigate to='/dashboard' replace /> },
			{ path: 'dashboard', element: <Dashboard /> },
			{ path: 'solicitudes', element: <SolicitudesList /> },
			{ path: 'solicitudes/nueva', element: <SolicitudForm /> },
			{ path: 'solicitudes/:id', element: <SolicitudDetail /> },
			{
				path: 'solicitudes/:id/editar',
				element: (
					<RutaPorRol
						rolesPermitidos={[
							'Administrador',
							'Analista',
							'Solicitante',
						]}>
						<SolicitudForm />
					</RutaPorRol>
				),
			},
			// La propiedad de la solicitud y la ventana de edición (solo en REGISTRADA)
			{ path: '*', element: <Navigate to='/dashboard' replace /> },
		],
	},
	{ path: '*', element: <Navigate to='/dashboard' replace /> },
]);
