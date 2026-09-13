import { z } from 'zod';

export const prioridadSchema = z.object({
  nombre: z.string().min(1, 'El nombre es obligatorio').max(100, 'Máximo 100 caracteres'),
  nivel: z.number('Ingrese un nivel válido').int().min(1, 'El nivel debe ser mayor a 0'),
});

export type PrioridadFormValues = z.infer<typeof prioridadSchema>;
