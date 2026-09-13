import { z } from 'zod';

// Edición completa de Administrador (PUT /api/solicitudes/{id}, ver ADR-0021): los cinco
// campos son obligatorios, a diferencia del PATCH parcial que usan las otras vistas.
// Máximos alineados a Solicitud.MAX_LONGITUD_TITULO (150) y MAX_LONGITUD_DESCRIPCION (2000).
export const solicitudAdminSchema = z.object({
  titulo: z.string().min(1, 'El título es obligatorio').max(150, 'Máximo 150 caracteres'),
  descripcion: z.string().min(1, 'La descripción es obligatoria').max(2000, 'Máximo 2000 caracteres'),
  tipoSolicitudId: z.number().int().min(1, 'Seleccione un tipo de solicitud'),
  prioridadId: z.number().int().min(1, 'Seleccione una prioridad'),
  areaId: z.number().int().min(1, 'Seleccione un área'),
});

export type SolicitudAdminFormValues = z.infer<typeof solicitudAdminSchema>;

// Evidencia opcional (ver ADR-0006 y ADR-0025): solo texto y URL, sin almacenamiento de
// archivos. La validación de formato de URL es deliberadamente laxa — un http(s):// basta,
// porque el requerimiento también admite una referencia de texto plano, y el backend
// (CrearAdjuntoCommandValidator) no exige un formato de URL. Se reutiliza tanto en el alta
// de solicitud como en el formulario "Agregar referencia" del detalle.
const camposEvidencia = {
  adjuntoDescripcion: z.string().max(200, 'Máximo 200 caracteres').optional().or(z.literal('')),
  adjuntoUrl: z
    .string()
    .max(500, 'Máximo 500 caracteres')
    .refine((valor) => valor === '' || /^https?:\/\//i.test(valor), 'Debe comenzar con http:// o https://')
    .optional()
    .or(z.literal('')),
};

// Alta de Solicitante/Analista (POST /api/solicitudes): mismos campos que la edición de
// Administrador, más la evidencia opcional. UsuarioSolicitanteId no viaja en el formulario:
// el backend lo toma del token (ver CrearSolicitudCommand).
export const solicitudCrearSchema = z
  .object({
    titulo: z.string().min(1, 'El título es obligatorio').max(150, 'Máximo 150 caracteres'),
    descripcion: z.string().min(1, 'La descripción es obligatoria').max(2000, 'Máximo 2000 caracteres'),
    tipoSolicitudId: z.number().int().min(1, 'Seleccione un tipo de solicitud'),
    prioridadId: z.number().int().min(1, 'Seleccione una prioridad'),
    areaId: z.number().int().min(1, 'Seleccione un área'),
    ...camposEvidencia,
  })
  .refine((valores) => !valores.adjuntoDescripcion || Boolean(valores.adjuntoUrl), {
    message: 'Ingrese la URL de la evidencia',
    path: ['adjuntoUrl'],
  });

export type SolicitudCrearFormValues = z.infer<typeof solicitudCrearSchema>;

// Formulario "Agregar referencia o URL" en el detalle de la solicitud (POST .../adjuntos):
// aquí sí es obligatoria, porque es el único propósito del formulario (a diferencia del
// alta de solicitud, donde la evidencia es un extra opcional entre otros campos).
export const adjuntoSchema = z.object({
  adjuntoDescripcion: z.string().max(200, 'Máximo 200 caracteres').optional().or(z.literal('')),
  adjuntoUrl: z
    .string()
    .min(1, 'La URL es obligatoria')
    .max(500, 'Máximo 500 caracteres')
    .regex(/^https?:\/\//i, 'Debe comenzar con http:// o https://'),
});

export type AdjuntoFormValues = z.infer<typeof adjuntoSchema>;
