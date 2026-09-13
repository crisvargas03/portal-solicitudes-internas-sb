import { z } from 'zod';

// Máximo alineado a Comentario.MAX_LONGITUD_TEXTO (1000). No hay campo de visibilidad aquí:
// el formulario de comentarios se usa desde el detalle para cualquier rol, pero solo
// Administrador/Analista pueden marcar "interno" (ver ADR-0023) — ese control vive en
// SolicitudDetail.tsx, no en este schema compartido.
export const comentarioSchema = z.object({
  texto: z.string().min(1, 'El comentario no puede estar vacío').max(1000, 'Máximo 1000 caracteres'),
});

export type ComentarioFormValues = z.infer<typeof comentarioSchema>;
