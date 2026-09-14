# ADR-0033: Diálogo de confirmación reutilizable (`ui/ConfirmDialog` + `useConfirm`)

**Estado:** Aceptada
**Fecha:** 2026-09-13

## Contexto

Una auditoría de todo el frontend (los tres roles) encontró cero infraestructura de confirmación:
ni `window.confirm`, ni un modal de tipo "¿estás seguro?", ni un guard de navegación en ningún
sitio. Toda acción destructiva —desactivar un área/prioridad/tipo/usuario, cambiar el rol de un
usuario, cerrar/reabrir una solicitud, reasignar o desasignar un responsable, cerrar sesión—
disparaba su mutación en el primer clic. Los cuatro modales existentes (`CambiarEstadoModal` y los
privados `ComentarioModal`/`EvidenciaModal`/`AsignarModal`) son todos modales de formulario, nunca
de confirmación.

De paso, la auditoría encontró que `alternarActivo` en los tres catálogos administrables
(`AreasAdminSection`, `PrioridadesAdminSection`, `TiposSolicitudAdminSection`) no tenía
`try/catch`, a diferencia de su `onSubmit` hermano: un rechazo del servidor salía como *unhandled
promise rejection*, sin toast ni banner.

No existe ningún endpoint `DELETE` en la aplicación — la baja es siempre lógica
(`activo: false`, ver ADR-0020 y ADR-0022) — así que el vocabulario de las confirmaciones es
siempre "desactivar", nunca "eliminar".

## Decisión

**`components/ui/ConfirmDialog.tsx`**, compuesto sobre el `ui/Modal` existente (ADR-0031 obliga a
alinearse a ese primitivo en vez de crear otro overlay), con props en español sin prefijo `I`:
`{ abierto, titulo, mensaje, textoConfirmar?, textoCancelar?, variante?: 'default' | 'peligro',
cargando?, onConfirmar, onCancelar }`. `variante='peligro'` usa una nueva variante `peligro` de
`ui/Button` (`bg-danger text-white`), reutilizando el mismo rojo que ya usan los banners de error
de formulario.

**`hooks/useConfirm.ts` + `store/confirmStore.ts`.** El proyecto no usa React Context en ningún
sitio (estado global es Zustand: `authStore`, `uiStore`). Para no introducir el primer Context
solo para esto, `useConfirm()` se apoya en un `confirmStore` de Zustand y un único
`<ConfirmDialogHost />` montado una vez en `App.tsx`, junto al `<Toaster />` — mismo patrón, sin
prop-drilling:

```ts
confirmar({
  titulo: string,
  mensaje: ReactNode,
  textoConfirmar?: string,
  textoCancelar?: string,
  variante?: 'default' | 'peligro',
  accion?: () => Promise<unknown>,
}) => Promise<boolean>
```

Si se pasa `accion`, el diálogo permanece abierto, deshabilita ambos botones y muestra el spinner
en Confirmar hasta que la promesa resuelve — necesario porque con un `Promise<boolean>` puro el
diálogo ya se habría cerrado antes de que la petición arrancara. Si `accion` lanza, el diálogo se
cierra y el error se re-lanza al `try/catch` del llamador (mismo lugar donde ya se manejaba antes
de existir el diálogo). Si se omite `accion` (caso "Cerrar sesión", sin petición que esperar),
resuelve `true` en el clic.

**Aplicado a 11 acciones** encontradas en la auditoría: cerrar sesión; desactivar área/prioridad/
tipo de solicitud (con el bug de `try/catch` corregido de paso); desactivar un usuario (con copy
distinto cuando el Administrador se desactiva a sí mismo); cambiar el rol de un usuario (ídem);
cerrar una solicitud y reabrir una solicitud cerrada; reasignar a otro analista y dejar
"Sin asignar". Deliberadamente **no** se confirma: reactivar (constructivo, mismo botón que
desactivar), crear, editar nombre/descripción, transiciones de estado intermedias (el `<select>`
ya obliga a elegir y el comentario ya se vuelve obligatorio cuando la transición lo exige — pedir
confirmación en cada una devaluaría el diálogo justo donde importa: el cierre), agregar comentario
o evidencia, y "Tomar" en la cola de Analista (flujo de trabajo principal, sin fricción).

**Detección de cierre/reapertura, dirigida por datos.** El docblock de `CambiarEstadoModal`
declara explícitamente que el componente nunca compara el destino contra `CERRADA` a mano — esa
regla vive en `TransicionPermitida` (ADR-0005). La confirmación respeta esto cruzando el destino
elegido contra `EstadoSolicitud.esFinal` (catálogo ya cacheado, ADR-0018), no contra el código.
Para reconocer "reabrir" hace falta el estado actual de la solicitud, que el modal no recibía —
se le agrega la prop `estadoActual: EstadoSolicitud`.

**Arreglo de `ui/Modal` para permitir anidamiento.** La confirmación de cierre se abre encima de
`CambiarEstadoModal`. El `Modal` tenía `id="modal-titulo"` hardcodeado (duplicado si hay dos
instancias) y ambos escuchaban `Escape` en `document` (una pulsación cerraba las dos). Se
reemplaza el id por `useId()` y se agrega un contador de montaje a nivel de módulo para que
Escape solo cierre el modal montado más recientemente.

## Consecuencias

- **A favor:** un mismo componente y un mismo hook cubren las 11 confirmaciones sin
  prop-drilling ni duplicar la mecánica de "abrir modal, deshabilitar botones, mostrar spinner,
  cerrar" en cada sitio; el copy queda específico por acción en vez de un "¿Estás seguro?"
  genérico; se corrige un bug real (falta de `try/catch`) al tocar los mismos handlers; la
  detección de estado final queda dirigida por datos, consistente con cómo ya funciona el resto
  del módulo de transiciones.
- **En contra / trade-offs:** `useConfirm` con `accion` opcional es una API un poco más grande que
  un `Promise<boolean>` puro — quien la usa debe decidir si pasa `accion` o maneja el `then`
  posterior a mano, y en los pocos sitios donde no se pasa `accion` (D2a/D2b, C2b) el llamador debe
  acordarse de comprobar el `boolean` devuelto antes de continuar, algo que un `try/catch` simple no
  fuerza. `ui/Modal` gana una pieza de estado a nivel de módulo (contador de montaje) que antes no
  tenía, aunque acotada a resolver Escape en anidamiento.
- **Alternativas descartadas:** un React Context nuevo para el diálogo — descartado porque
  hubiera sido el primer `createContext` del proyecto, rompiendo con el patrón 100% Zustand ya
  establecido, sin ninguna ventaja sobre un store más para este caso puntual; comparar
  `codigo === 'CERRADA'` en vez de `esFinal` — descartado porque contradice explícitamente la
  regla que el propio código documenta y deja de funcionar si se agrega otro estado final;
  confirmar también "Tomar" y el comentario público — descartado tras revisar la auditoría con el
  autor: ambos son flujo de trabajo normal y confirmarlos entrena a hacer clic sin leer justo donde
  el diálogo más importa.
