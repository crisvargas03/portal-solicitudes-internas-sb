import { z } from 'zod';

// El '|' separa campos en el archivo que respalda este catalogo (ver ADR-0034 y
// EntidadGubernamentalValidacionExtensions en el backend): el backend lo rechaza en
// cualquier campo de texto, asi que se valida tambien aqui para dar el error antes del submit.
const campoDeArchivo = (maxLength: number) =>
  z
    .string()
    .min(1, 'Campo obligatorio')
    .max(maxLength, `Máximo ${maxLength} caracteres`)
    .refine((valor) => !valor.includes('|'), "No puede contener el carácter '|'");

export const entidadGubernamentalSchema = z.object({
  nombre: campoDeArchivo(200),
  categoria: campoDeArchivo(100),
  poderDelEstado: campoDeArchivo(100),
  sector: campoDeArchivo(100),
});

export type EntidadGubernamentalFormValues = z.infer<typeof entidadGubernamentalSchema>;
