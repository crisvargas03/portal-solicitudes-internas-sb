import { z } from 'zod';

// Máximo alineado a Comentario.MAX_LONGITUD_TEXTO (1000). `esInterno` viaja siempre en el
// formulario, pero solo Administrador/Analista tienen el checkbox para marcarlo (ver ADR-0023);
// para Solicitante el control ni se renderiza y el valor por defecto (false) es el que se envía.
export const comentarioSchema = z.object({
  texto: z.string().min(1, 'El comentario no puede estar vacío').max(1000, 'Máximo 1000 caracteres'),
  esInterno: z.boolean(),
});

export type ComentarioFormValues = z.infer<typeof comentarioSchema>;
