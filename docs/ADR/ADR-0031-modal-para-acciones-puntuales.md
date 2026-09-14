# ADR-0031: `ui/Modal` headless para acciones puntuales sobre una fila

**Estado:** Aceptada
**Fecha:** 2026-09-13

## Contexto

Hasta ahora todo formulario de edición/creación del frontend es un panel inline (secciones de
`AreasAdminSection`, "Asignar responsable" en `SolicitudDetail.tsx`) — no existía ningún
componente modal/diálogo en el proyecto. "Cambiar estado" necesita un formulario corto
(seleccionar destino + comentario condicional) disparado desde una fila de una cola paginada
(`AnalistaQueueView`) y también desde el detalle de la solicitud; expandir la fila in-place no
es viable en una lista paginada y duplicar el formulario en dos layouts distintos tampoco.

## Decisión

Se agrega `components/ui/Modal.tsx`: headless, sin librería externa, con `createPortal` a
`document.body`, cierre por `Escape` o clic en el backdrop, foco inicial en el panel y atributos
`role="dialog"` / `aria-modal` / `aria-labelledby`. Props en español sin prefijo `I`
(`{ abierto, titulo, onCerrar, children }`), igual convención que el resto de `ui/`. Se usa una
sola vez por ahora: `CambiarEstadoModal`, compartido entre `AnalistaQueueView` y
`SolicitudDetail`.

## Consecuencias

- **A favor:** un mismo formulario de cambio de estado sirve para la cola y el detalle sin
  duplicar lógica; queda un primitivo reutilizable para el próximo caso que lo necesite, en vez
  de resolver cada uno con su propio overlay ad hoc.
- **En contra / trade-offs:** introduce el primer patrón de overlay del proyecto — cualquier
  futuro modal debe alinearse a este primitivo en vez de crear otro (foco, teclado, backdrop) por
  su cuenta.
- **Alternativas descartadas:** panel inline expandido en la fila de la cola, descartado porque
  no tiene una forma razonable de coexistir con la paginación server-side ni de reusarse tal cual
  en `SolicitudDetail`, que no tiene filas.
