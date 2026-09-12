import type { Solicitud } from '../types';

const MOCK_SOLICITUDES: Solicitud[] = [];

export async function getSolicitudes(): Promise<Solicitud[]> {
  return MOCK_SOLICITUDES;
}

export async function getSolicitudById(id: number): Promise<Solicitud | undefined> {
  return MOCK_SOLICITUDES.find((solicitud) => solicitud.id === id);
}
