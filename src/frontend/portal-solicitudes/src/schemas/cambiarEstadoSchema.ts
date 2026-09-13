import { z } from 'zod';

// PATCH /api/solicitudes/{id}/estado. `requiereComentario` no es un campo del formulario: viaja
// oculto para que el refine sepa, por cada transición seleccionada, si el comentario es
// obligatorio (mismo patrón condicional que solicitudCrearSchema con la evidencia opcional).
// El backend es la fuente de verdad de esa bandera (TransicionPermitida.RequiereComentario,
// ver ADR-0005) — aquí solo se refleja lo que ya vino en GET .../transiciones.
export const cambiarEstadoSchema = z
  .object({
    estadoDestinoId: z.number().int().min(1, 'Seleccione un estado'),
    comentario: z.string().max(1000, 'Máximo 1000 caracteres'),
    requiereComentario: z.boolean(),
  })
  .refine((valores) => !valores.requiereComentario || valores.comentario.trim().length > 0, {
    message: 'Este cambio de estado requiere un comentario',
    path: ['comentario'],
  });

export type CambiarEstadoFormValues = z.infer<typeof cambiarEstadoSchema>;
