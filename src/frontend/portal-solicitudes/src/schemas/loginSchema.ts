import { z } from 'zod';

// El backend valida NotEmpty().EmailAddress() (ver IniciarSesionCommandValidator) — por eso
// es correo, no usuario, y por eso no se agrega un min(8) en password: una regla de longitud
// en cliente rechazaría credenciales válidas que ya existen en la base de datos.
export const loginSchema = z.object({
  email: z.string().min(1, 'El correo es obligatorio').email('Ingrese un correo válido'),
  password: z.string().min(1, 'La contraseña es obligatoria'),
});

export type LoginFormValues = z.infer<typeof loginSchema>;
