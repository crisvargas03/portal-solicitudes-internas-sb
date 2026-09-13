import { z } from 'zod';

export const areaSchema = z.object({
  nombre: z.string().min(1, 'El nombre es obligatorio').max(100, 'Máximo 100 caracteres'),
});

export type AreaFormValues = z.infer<typeof areaSchema>;
