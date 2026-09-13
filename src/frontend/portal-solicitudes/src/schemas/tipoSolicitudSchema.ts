import { z } from 'zod';

export const tipoSolicitudSchema = z.object({
  nombre: z.string().min(1, 'El nombre es obligatorio').max(100, 'Máximo 100 caracteres'),
  descripcion: z.string().max(500, 'Máximo 500 caracteres').optional().or(z.literal('')),
});

export type TipoSolicitudFormValues = z.infer<typeof tipoSolicitudSchema>;
