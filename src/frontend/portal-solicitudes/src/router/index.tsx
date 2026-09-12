import { createBrowserRouter, Navigate } from 'react-router';
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
    element: <AppLayout />,
    children: [
      { index: true, element: <Navigate to="/dashboard" replace /> },
      { path: 'dashboard', element: <Dashboard /> },
      { path: 'solicitudes', element: <SolicitudesList /> },
      { path: 'solicitudes/nueva', element: <SolicitudForm /> },
      { path: 'solicitudes/:id', element: <SolicitudDetail /> },
      { path: 'solicitudes/:id/editar', element: <SolicitudForm /> },
    ],
  },
]);
