import { z } from 'zod';

// El backend exige min(8) en Password (ver CrearUsuarioCommandValidator) — aqui si aplica,
// a diferencia de loginSchema, porque es la misma regla que valida el alta.
export const crearUsuarioSchema = z.object({
  nombre: z.string().min(1, 'El nombre es obligatorio').max(150, 'Máximo 150 caracteres'),
  email: z.string().min(1, 'El correo es obligatorio').email('Ingrese un correo válido'),
  password: z.string().min(8, 'Mínimo 8 caracteres'),
  rol: z.enum(['Administrador', 'Analista', 'Solicitante']),
});

export type CrearUsuarioFormValues = z.infer<typeof crearUsuarioSchema>;

export const editarUsuarioSchema = z.object({
  nombre: z.string().min(1, 'El nombre es obligatorio').max(150, 'Máximo 150 caracteres'),
  rol: z.enum(['Administrador', 'Analista', 'Solicitante']),
  activo: z.boolean(),
});

export type EditarUsuarioFormValues = z.infer<typeof editarUsuarioSchema>;
