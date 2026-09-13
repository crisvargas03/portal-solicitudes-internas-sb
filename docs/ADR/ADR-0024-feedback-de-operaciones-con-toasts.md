# ADR-0024: Feedback de éxito/error de mutaciones con toasts (`sonner`)

**Estado:** Aceptada
**Fecha:** 2026-09-13

## Contexto

El único patrón de feedback existente en el frontend era un banner inline (`bg-danger-soft`) para
errores de formulario, mostrado junto al campo o al pie del form, y la navegación como única señal
de éxito. No existía ningún mecanismo para confirmar el éxito de una mutación que no navega a otra
pantalla — agregar un comentario o una referencia de evidencia desde el detalle de una solicitud
se queda en la misma página, y sin una señal explícita el usuario no tiene forma de saber si el
envío funcionó más que notar que la lista se actualizó.

Las opciones eran: extender el banner inline también para éxito, agregar una librería de toasts, o
combinar ambas cosas según el tipo de interacción.

## Decisión

Se agrega `sonner` como dependencia y se monta un `<Toaster richColors position="top-right" />` en
`App.tsx`. El uso se divide por tipo de interacción, para que los dos mecanismos no compitan por
el mismo caso:

- **Mutaciones in-place** (agregar comentario, agregar adjunto): `toast.success`/`toast.error`,
  porque el usuario permanece en la misma pantalla y una notificación transitoria es suficiente.
- **Envíos de formulario de página completa** (alta/edición de solicitud): se mantiene el banner
  inline `errorGeneral` para el error (queda visible mientras el usuario corrige, no desaparece
  solo) y se agrega `toast.success` para el éxito, ya que ese caso sí navega a otra pantalla y el
  banner de la página anterior no se vería.

## Consecuencias

- **A favor:** cada mecanismo cubre el caso para el que es mejor — el toast no se pierde en una
  navegación y el banner no desaparece antes de que el usuario termine de leer el error mientras
  sigue en el formulario. No hace falta rediseñar el patrón de error ya establecido en
  Login/AdminUsuarios/AreasAdminSection.
- **En contra / trade-offs:** se agrega una dependencia nueva al proyecto y dos mecanismos de
  feedback en paralelo en vez de uno solo — quien mantenga el código debe saber cuál corresponde a
  cada caso en lugar de que exista una única respuesta.
- **Alternativas descartadas:** usar toasts para todo, incluida la validación de formularios,
  descartada porque el error de campo debe quedar anclado al campo (vía `FormField`) y no
  desaparecer solo — un toast que se autodestruye no es un buen lugar para un error que el usuario
  necesita seguir viendo mientras corrige. Extender solo el banner inline para todos los casos,
  descartada porque no hay banner que mostrar cuando la mutación es in-place y no hay page load de
  por medio para que el usuario lo note.
