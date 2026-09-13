import { z } from 'zod';

// Edición completa de Administrador (PUT /api/solicitudes/{id}, ver ADR-0021): los cinco
// campos son obligatorios, a diferencia del PATCH parcial que usan las otras vistas.
export const solicitudAdminSchema = z.object({
  titulo: z.string().min(1, 'El título es obligatorio').max(200, 'Máximo 200 caracteres'),
  descripcion: z.string().min(1, 'La descripción es obligatoria').max(2000, 'Máximo 2000 caracteres'),
  tipoSolicitudId: z.number().int().min(1, 'Seleccione un tipo de solicitud'),
  prioridadId: z.number().int().min(1, 'Seleccione una prioridad'),
  areaId: z.number().int().min(1, 'Seleccione un área'),
});

export type SolicitudAdminFormValues = z.infer<typeof solicitudAdminSchema>;
